using Newtonsoft.Json;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Utils;
using PolyPaint.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using PolyPaint.Services.Storage;
using System.Net;
using System.Linq;

namespace PolyPaint.Services.Cache
{
    public interface ICacheService
    {
        T Get<T>(string path, string name);
        Dictionary<string, T> GetAll<T>(string path);
        List<string> GetAllIds(string path);
        string GetCachedResource(string url, string fallback);
        void UpdateResource(string url, Stream file);

        void Write<T>(string folder, string key, T obj);
    }

    public class CacheService : ICacheService
    {
        private ILogger Logger { get; }
        private IStorageService Storage { get; }
        private IDatabaseService Database { get; }
        private IAuthenticationService AuthService { get; }
        private CancellationTokenSource CancellationTokenSource { get; set; }

        private Dictionary<string, string> StorageMapping { get; set; }

        private bool IsReachable => NetworkInterface.GetIsNetworkAvailable();
        private bool IsAuthenticated => AuthService.IsLoggedIn;

        public CacheService(ILogger logger, IAuthenticationService authService, IDatabaseService database, IStorageService storage)
        {
            Logger = logger;
            Storage = storage;
            Database = database;
            AuthService = authService;

            Logger.Debug($"Creating app data folder : {Paths.AppData}");
            Directory.CreateDirectory(Paths.AppData);

            Directory.CreateDirectory(Paths.Cache);
            Directory.CreateDirectory(Paths.StorageCache);
            Directory.CreateDirectory(Paths.DatabaseCache);

            Directory.CreateDirectory(Paths.Override);
            Directory.CreateDirectory(Paths.StorageOverride);
            Directory.CreateDirectory(Paths.DatabaseOveride);

            Directory.CreateDirectory(Paths.Tmp);

            StorageMapping = GetStorageMapping();

            StateChanged(AuthService?.CurrentUser);
            AuthService.CurrentUserChanged += StateChanged;
        }

        private async void StateChanged(User user)
        {
            if (user == null)
            {
                if (CancellationTokenSource?.Token.CanBeCanceled ?? false)
                {
                    CancellationTokenSource.Cancel();
                }
            }
            else
            {
                await UploadUpdatedData();
                ClearOverride();

                CancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014
                Task.Run(() => SyncThread(CancellationTokenSource.Token));
#pragma warning restore CS4014
            }
        }

        private async Task UploadUpdatedData()
        {
            var drawings = GetAllOverriden<DrawingModel>(DatabasePaths.Drawings);
            var drawingsInfo = GetAllOverriden<DrawingInfo>(DatabasePaths.DrawingInfo);
            var strokes = GetAllOverriden<Dictionary<string, StrokeModel>>(DatabasePaths.Strokes);

            var updatedDrawings = new Dictionary<string, string>();

            var hashes = GetHashes(DatabasePaths.DrawingInfo);
            foreach (var id in drawingsInfo.Keys)
            {
                var localDrawingInfo = drawingsInfo[id];
                var onlineDrawingInfo = await Database.Ref(DatabasePaths.DrawingInfo).Child(id).Once<DrawingInfo>();
                if (onlineDrawingInfo == null || hashes[id] == GenerateHash(onlineDrawingInfo))
                {
                    updatedDrawings[id] = id;
                    localDrawingInfo.PreviewUrl = await UploadFile(StoragePaths.Drawings, localDrawingInfo.Id + ".jpg", localDrawingInfo.PreviewUrl);
                    localDrawingInfo.ThumbnailUrl = await UploadFile(StoragePaths.Thumbnails, localDrawingInfo.Id + "_thumbnail.jpg", localDrawingInfo.ThumbnailUrl);

                    if (localDrawingInfo.Owner == AuthService.OfflineClientId)
                    {
                        localDrawingInfo.Owner = AuthService?.CurrentUser?.Id;
                        if (drawings.ContainsKey(id))
                        {
                            drawings[id].Owner = AuthService?.CurrentUser?.Id;
                        }
                    }

                    if (drawings.ContainsKey(id))
                    {
                        await Database.Ref(DatabasePaths.Drawings).Child(id).Set(drawings[id]);
                    }

                    if (strokes.ContainsKey(id))
                    {
                        await Database.Ref(DatabasePaths.Strokes).Child(id).Set(strokes[id]);
                    }

                    await Database.Ref(DatabasePaths.DrawingInfo).Child(id).Set(drawingsInfo[id]);
                }
            }

            await Database.Ref(DatabasePaths.Users).Child(AuthService.CurrentUser.Id).Child(DatabasePaths.Drawings).Update(updatedDrawings);
        }

        private void ClearOverride()
        {
            try
            {
                Directory.Delete(Paths.Override, true);

                Directory.CreateDirectory(Paths.Override);
                Directory.CreateDirectory(Paths.StorageOverride);
                Directory.CreateDirectory(Paths.DatabaseOveride);
            }
            catch (Exception) { }
        }

        public T Get<T>(string path, string name)
        {
            var cachePath = Path.Combine(Paths.DatabaseCache, path.Base32Encode(), name.Base32Encode());
            var overridePath = Path.Combine(Paths.DatabaseOveride, path.Base32Encode(), name.Base32Encode());

            if (File.Exists(overridePath))
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(overridePath));
            }
            else if (File.Exists(cachePath))
            {
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(cachePath));
            }

            return default(T);
        }

        public Dictionary<string, T> GetAll<T>(string target)
        {
            var cachePath = Path.Combine(Paths.DatabaseCache, target.Base32Encode());
            var overridePath = Path.Combine(Paths.DatabaseOveride, target.Base32Encode());

            var objs = new Dictionary<string, T>();
            if (Directory.Exists(cachePath))
            {
                foreach (var file in Directory.GetFiles(cachePath))
                {
                    var obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
                    objs[Path.GetFileName(file).Base32Decode()] = obj;
                }
            }

            if (Directory.Exists(overridePath))
            {
                foreach (var file in Directory.GetFiles(overridePath))
                {
                    var obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
                    objs[Path.GetFileName(file).Base32Decode()] = obj;
                }
            }

            return objs;
        }

        public Dictionary<string, T> GetAllOverriden<T>(string target)
        {
            var overridePath = Path.Combine(Paths.DatabaseOveride, target.Base32Encode());

            var objs = new Dictionary<string, T>();
            if (Directory.Exists(overridePath))
            {
                foreach (var file in Directory.GetFiles(overridePath))
                {
                    var obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(file));
                    objs[Path.GetFileName(file).Base32Decode()] = obj;
                }
            }

            return objs;
        }

        public List<string> GetAllIds(string path)
        {
            var cachePath = Path.Combine(Paths.DatabaseCache, path.Base32Encode());
            var overridePath = Path.Combine(Paths.DatabaseOveride, path.Base32Encode());

            var ids = new HashSet<string>();

            if (Directory.Exists(cachePath))
            {
                foreach (var file in Directory.GetFiles(GetFolderPathForTarget(path)))
                {
                    ids.Add(Path.GetFileName(file).Base32Decode());
                }
            }

            if (Directory.Exists(overridePath))
            {
                foreach (var file in Directory.GetFiles(overridePath))
                {
                    ids.Add(Path.GetFileName(file).Base32Decode());
                }
            }

            return ids.ToList(); ;
        }

        public void Write<T>(string folder, string key, T obj)
        {
            Write(folder, key, obj, null, saveToOverride: true);
        }

        public string GetCachedResource(string url, string fallback)
        {
            if (String.IsNullOrWhiteSpace(url) || !StorageMapping.ContainsKey(url))
                return fallback;

            string filePath = null;
            if (File.Exists(Path.Combine(Paths.StorageOverride, StorageMapping[url])))
            {
                filePath = Path.Combine(Paths.StorageOverride, StorageMapping[url]);
            }
            else if (File.Exists(Path.Combine(Paths.StorageCache, StorageMapping[url])))
            {
                filePath = Path.Combine(Paths.StorageCache, StorageMapping[url]);
            }

            if (filePath != null)
            {
                var tmpFilePath = Path.Combine(Paths.Tmp, Guid.NewGuid().ToString());
                File.Copy(filePath, tmpFilePath);
                filePath = tmpFilePath;
            }

            return filePath ?? fallback;
        }

        public void UpdateResource(string url, Stream file)
        {
            UpdateCachedResource(url, file, true);
        }

        private void UpdateCachedResource(string url, Stream file, bool saveToOverride = false)
        {
            if (String.IsNullOrWhiteSpace(url))
                return;

            StorageMapping[url] = Guid.NewGuid().ToString();
            var path = Path.Combine(saveToOverride ? Paths.StorageOverride : Paths.StorageCache, StorageMapping[url]);
            using (Stream localFile = File.Create(path))
            {
                file.CopyTo(localFile);
            }

            UpdateMapping();
        }

        private async Task SyncThread(CancellationToken token)
        {
            Logger.Info($"Drawing Sync Service's sync thread started. Using folder {Paths.Cache}.");
            bool syncedStorageOnce = false;

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    Logger.Info("Drawing Sync Service's sync cancellation was requested.");
                    break;
                }

                Thread.Sleep(Constants.SyncInterval);

                if (!IsReachable || !IsAuthenticated)
                    continue;

                if (!syncedStorageOnce)
                {
                    await DownloadStorage();
                    syncedStorageOnce = true;
                }

                try
                {
                    await SlowSyncDatabaseRef<object>(DatabasePaths.Drawings);
                    await SlowSyncDatabaseRef<DrawingInfo>(DatabasePaths.DrawingInfo, (o) =>
                    {
                        DownloadFile(o.PreviewUrl);
                        DownloadFile(o.ThumbnailUrl);
                    });

                    UpdateMapping();

                    await SlowSyncDatabaseRef<object>(DatabasePaths.Strokes);
                    await SlowSyncDatabaseRef<object>(DatabasePaths.Users);
                }
                catch (Exception ex)
                {
                    Logger.Error("An error occured while syncing data to local cache.", ex);
                }
            }
        }

        private async Task SlowSyncDatabaseRef<T>(string target, Action<T> onChildChanged = null)
        {
            var hashes = GetHashes(target);
            var path = GetFolderPathForTarget(target);
            var objs = await Database.Ref(target).Once<Dictionary<string, T>>();
            if (objs == null)
                return;

            foreach (var obj in objs)
            {
                var currentHash = hashes.ContainsKey(obj.Key) ? hashes[obj.Key] : String.Empty;
                var newHash = Write(target, obj.Key, obj.Value, currentHash);

                if (currentHash != newHash && onChildChanged != null)
                {
                    onChildChanged(obj.Value);
                }

                hashes[obj.Key] = newHash;
                Thread.Sleep(Constants.SaveInterval);
            }

            // TODO : Test this when we merge the delete method
            foreach (var file in Directory.GetFiles(path))
            {
                if (objs.ContainsKey(Path.GetFileName(file).Base32Decode()))
                    continue;

                File.Delete(file);
            }

            UpdateHashes(target, hashes);
        }

        private async Task<string> UploadFile(string target, string uploadedFileName, string url)
        {
            var filePath = GetCachedResource(url, null);

            if (filePath == null || !File.Exists(filePath))
                return null;

            using (var file = new FileStream(filePath, FileMode.Open))
            {
                try
                {
                    await Storage.Ref(target).Child(uploadedFileName).Put(file);
                }
                catch (Exception ex)
                {
                    Logger.Warn("An error occured while uploading file {filePath}", ex);
                }
            }

            return await Storage.Ref(target).Child(uploadedFileName).GetDownloadUrl();
        }

        private async Task DownloadStorage()
        {
            var mapping = GetStorageMapping();
            var drawings = await Database.Ref(DatabasePaths.DrawingInfo).Once<Dictionary<string, DrawingInfo>>();
            if (drawings == null)
                return;

            foreach (var drawing in drawings?.Values)
            {
                DownloadFile(drawing.PreviewUrl);
                DownloadFile(drawing.ThumbnailUrl);
            }

            UpdateMapping();
        }

        private string Write<T>(string folder, string key, T obj, string previousHash, bool saveToOverride = false)
        {
            var data = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            var newHash = GenerateHash(data);
            if (newHash == previousHash) { return previousHash; }

            var directory = Directory.CreateDirectory(Path.Combine(saveToOverride ? Paths.DatabaseOveride : Paths.DatabaseCache, folder.Base32Encode()));
            var path = Path.Combine(directory.FullName, key.Base32Encode());
            File.WriteAllText(path, data);

            return newHash;
        }

        private void DownloadFile(string url)
        {
            if (String.IsNullOrWhiteSpace(url))
                return;

            using (WebClient client = new WebClient())
            {
                if (!StorageMapping.ContainsKey(url))
                {
                    StorageMapping[url] = Guid.NewGuid().ToString();
                }
                var path = Path.Combine(Paths.StorageCache, StorageMapping[url]);

                try
                {
                    client.DownloadFile(new Uri(url), path);
                }
                catch (Exception ex)
                {
                    Logger.Warn("An error occured while downloading a file.", ex);
                }
            }
        }

        private void DeleteFile(string url, Dictionary<string, string> mapping)
        {
            if (String.IsNullOrWhiteSpace(url) || !mapping.ContainsKey(url))
                return;

            var path = Path.Combine(Paths.StorageCache, mapping[url]);
            File.Delete(path);
        }

        private Dictionary<string, string> GetHashes(string targetPath)
        {
            var hashesFilePath = GetHashesFileName(targetPath);
            return File.Exists(hashesFilePath)
                 ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(hashesFilePath))
                 : new Dictionary<string, string>();
        }

        private Dictionary<string, string> GetStorageMapping()
        {
            var mapPath = Path.Combine(Paths.AppData, "storage_map");
            return File.Exists(mapPath)
                 ? JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(mapPath))
                 : new Dictionary<string, string>();
        }

        private string GetHashesFileName(string databasePath)
        {
            var fileName = databasePath.Base32Encode() + Constants.HashesFileSuffix;
            return Path.Combine(Paths.AppData, fileName);
        }

        private string GetFolderPathForTarget(string target)
        {
            return Path.Combine(Paths.DatabaseCache, target.Base32Encode());
        }

        private void UpdateHashes(string targetPath, Dictionary<string, string> hashes)
        {
            var fileName = targetPath.Base32Encode() + Constants.HashesFileSuffix;
            var hashesFilePath = Path.Combine(Paths.AppData, fileName);
            File.WriteAllText(hashesFilePath, JsonConvert.SerializeObject(hashes));
        }

        private void UpdateMapping()
        {
            var mapPath = Path.Combine(Paths.AppData, "storage_map");
            File.WriteAllText(mapPath, JsonConvert.SerializeObject(StorageMapping));
        }

        private string GenerateHash(object obj)
        {
            var serializedData = JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            return GenerateHash(serializedData);
        }

        private string GenerateHash(string serializedObject)
        {
            using (var md5 = MD5.Create())
            {
                var bytes = Encoding.ASCII.GetBytes(serializedObject);
                return Encoding.ASCII.GetString(md5.ComputeHash(bytes));
            }
        }

        private static class Constants
        {
            public readonly static int SyncInterval = 1;
            public readonly static int SaveInterval = 1;
            public readonly static string HashesFileSuffix = "_hashes";
        }

        private static class Paths
        {
            public static string AppData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Assembly.GetEntryAssembly().GetName().Name);

            public static readonly string Cache = Path.Combine(AppData, "cache");
            public static readonly string StorageCache = Path.Combine(Cache, "storage");
            public static readonly string DatabaseCache = Path.Combine(Cache, "database");

            public static readonly string Override = Path.Combine(AppData, "override");
            public static readonly string StorageOverride = Path.Combine(Override, "storage");
            public static readonly string DatabaseOveride = Path.Combine(Override, "database");

            public static readonly string Tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }
    }
}
