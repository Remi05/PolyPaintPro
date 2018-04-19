using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using PolyPaint.Extensions;
using PolyPaint.Models;
using PolyPaint.Services.Achievements;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Cache;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Storage;
using PolyPaint.Utils;

namespace PolyPaint.Services.Drawing
{
    public class DrawingService : IDrawingService
    {
        private IAuthenticationService AuthService { get; }
        private IDatabaseService DatabaseService { get; }
        private ILogger Logger { get; }
        private ICacheService Cache { get; }
        private IStorageService StorageService { get; }
        private ISubscription DrawingAddedSubscription { get; set; }
        private IAchievementsService AchievementsService { get; set; }

        public DrawingService(IAuthenticationService authService,
                                    IDatabaseService databaseService,
                                    ICacheService cache,
                                    ILogger logger,
                                    IAchievementsService achievementsService,
                                    IStorageService storageService)
        {
            AuthService = authService;
            DatabaseService = databaseService;
            Logger = logger;
            AchievementsService = achievementsService;
            Cache = cache;
            StorageService = storageService;
        }

        ~DrawingService()
        {
            DrawingAddedSubscription?.Stop();
        }

        public async Task<Drawing> CreateDrawing(bool isPublic, bool isProtected, SecureString password)
        {
            Logger.Info($"Creating Drawing for user ${AuthService.CurrentUser?.DisplayName}");

            var drawingModel = new DrawingModel()
            {
                Width = Constants.DefaultDrawingWidth,
                Height = Constants.DefaultDrawingHeight,
                IsPublic = isPublic,
                IsProtected = isProtected,
                Password = isProtected ? password?.ToUnsecureString() : null,
                Owner = AuthService?.CurrentUser?.Id
            };

            string drawingId;
            if (!AuthService.IsLoggedIn)
            {
                drawingId = Guid.NewGuid().ToString();
            }
            else
            {
                var drawingCreationQuery = await DatabaseService.Ref(DatabasePaths.Drawings).Push();
                await drawingCreationQuery.Set(drawingModel);
                drawingId = drawingCreationQuery.Key;
                await CreateDrawingInfo(drawingId);
            }

            return new Drawing(drawingId, drawingModel, AuthService, Cache, DatabaseService, Logger);
        }

        public async Task<Drawing> GetDrawing(string drawingId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting Drawing ${drawingId} while logged out.");
                return null;
            }

            Logger.Info($"Getting Drawing ${drawingId} for user ${AuthService.CurrentUser.DisplayName}");

            var drawingModel = await DatabaseService.Ref(DatabasePaths.Drawings).Child(drawingId).Once<DrawingModel>();
            return new Drawing(drawingId, drawingModel, AuthService, Cache, DatabaseService, Logger);
        }

        private async Task CreateDrawingInfo(string drawingId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried creating DrawingInfo ${drawingId} while logged out.");
                return;
            }

            Logger.Info($"Creating DrawingInfo ${drawingId} for user ${AuthService.CurrentUser.DisplayName}");

            var drawingInfo = new DrawingInfo()
            {
                Id = drawingId,
                LastModifiedOn = DateTime.Now,
                IsNsfw = false,
                Owner = AuthService?.CurrentUser?.Id
            };

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                 .Child(drawingId)
                                 .Set(drawingInfo);

            await DatabaseService.Ref(DatabasePaths.Users)
                                 .Child(AuthService.CurrentUser.Id)
                                 .Child(DatabasePaths.Drawings)
                                 .Child(drawingId)
                                 .Set(drawingId);
        }

        public async Task<DrawingInfo> GetDrawingInfo(string drawingId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting DrawingInfo ${drawingId} while logged out.");
                return null;
            }

            Logger.Info($"Getting DrawingInfo ${drawingId} for user ${AuthService.CurrentUser.DisplayName}");

            return await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                        .Child(drawingId)
                                        .Once<DrawingInfo>();
        }

        public async Task SaveDrawingPreview(string drawingId, Stream drawingStream, Stream thumbnailStream)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried saving drawing preview for drawing ${drawingId} while logged out.");
                return;
            }

            Logger.Info($"Saving drawing preview ${drawingId} for user ${AuthService.CurrentUser.DisplayName}");

            string fileName = $"{drawingId}.jpg";
            string thumbnailFileName = $"{drawingId}_thumbnail.jpg";

            await StorageService.Ref(StoragePaths.Drawings)
                                .Child(fileName)
                                .Put(drawingStream);

            await StorageService.Ref(StoragePaths.Thumbnails)
                       .Child(thumbnailFileName)
                       .Put(thumbnailStream);

            string previewUrl = await StorageService.Ref(StoragePaths.Drawings)
                                                    .Child(fileName)
                                                    .GetDownloadUrl();

            string thumbnailUrl = await StorageService.Ref(StoragePaths.Thumbnails)
                                                      .Child(thumbnailFileName)
                                                      .GetDownloadUrl();

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                 .Child(drawingId)
                                 .Child(DatabasePaths.PreviewUrl)
                                 .Set(previewUrl);

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                 .Child(drawingId)
                                 .Child(DatabasePaths.ThumbnailUrl)
                                 .Set(thumbnailUrl);

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                 .Child(drawingId)
                                 .Child(DatabasePaths.LastModifiedOn)
                                 .Set(DateTime.Now);
        }

        public async Task SetIsDrawingLiked(string drawingId, bool isLiked)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried liking/unliking drawing ${drawingId} while logged out.");
                return;
            }

            Logger.Info($"Setting drawing ${drawingId} isLiked to ${isLiked} for user ${AuthService.CurrentUser.DisplayName}");

            bool? value = isLiked ? true : (bool?)null;
            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                .Child(drawingId)
                                .Child(DatabasePaths.Likes)
                                .Child(AuthService.CurrentUser.Id)
                                .Set(value);
        }

        public async Task SetIsDrawingNsfw(string drawingId, bool isNsfw)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried updating isNsfw for drawing ${drawingId} while logged out.");
                return;
            }

            await AchievementsService.Increment(AuthService.CurrentUser.Id, AchievementMetrics.NumberOfNsfwDrawings);

            Logger.Info($"Setting drawing ${drawingId} isNsfw to ${isNsfw} for user ${AuthService.CurrentUser.DisplayName}");

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                 .Child(drawingId)
                                 .Child(DatabasePaths.Nsfw)
                                 .Set(isNsfw);
        }

        public async Task Report(string drawingId, string reason)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried reporting drawing ${drawingId} while logged out.");
                return;
            }

            Logger.Info($"Reporting drawing ${drawingId} for user ${AuthService.CurrentUser.DisplayName}");

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                .Child(drawingId)
                                .Child(DatabasePaths.Reports)
                                .Child(AuthService.CurrentUser.Id)
                                .Set(reason);
        }

        public async Task UndoReport(string drawingId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried unreporting drawing ${drawingId} while logged out.");
                return;
            }

            Logger.Info($"Unreporting drawing ${drawingId} for user ${AuthService.CurrentUser.DisplayName}");

            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                .Child(drawingId)
                                .Child(DatabasePaths.Reports)
                                .Child(AuthService.CurrentUser.Id)
                                .Remove();
        }

        public async Task<List<string>> GetPublicDrawingsIds(bool includeProtected = true)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting public drawing IDs while logged out.");
                return new List<string>();
            }

            Logger.Info($"Getting public drawings IDs for user ${AuthService.CurrentUser.DisplayName}");

            var drawingInfo = await DatabaseService.Ref(DatabasePaths.DrawingInfo).Once<Dictionary<string, DrawingInfo>>();
            var drawings = await DatabaseService.Ref(DatabasePaths.Drawings).Once<Dictionary<string, DrawingModel>>();

            var drawingIds = drawings?.Where(x => x.Value.IsPublic
                                             && drawingInfo.ContainsKey(x.Key)
                                             && !drawingInfo[x.Key].IsBanned
                                             && (includeProtected || !x.Value.IsProtected))
                                      .Select(x => x.Key)
                                      .ToList();

            if (drawingIds == null)
                return new List<string>();

            drawingIds.Sort((a, b) => drawingInfo[b].LastModifiedOn.CompareTo(drawingInfo[a].LastModifiedOn));

            return drawingIds;
        }

        public async Task<List<string>> GetDrawingsIds(string userId, bool includePrivates = false)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting drawing IDs for user ${userId} while logged out.");
                return new List<string>();
            }

            Logger.Info($"Getting drawings IDs for user ${AuthService.CurrentUser.DisplayName}");

            var userDrawingIds = await DatabaseService.Ref(DatabasePaths.Users)
                                                      .Child(userId)
                                                      .Child(DatabasePaths.Drawings)
                                                      .Once<Dictionary<string, string>>();

            var drawingInfo = await DatabaseService.Ref(DatabasePaths.DrawingInfo).Once<Dictionary<string, DrawingInfo>>();
            var drawings = await DatabaseService.Ref(DatabasePaths.Drawings).Once<Dictionary<string, DrawingModel>>();

            var drawingIds = userDrawingIds?.Where(x => (drawingInfo?.ContainsKey(x.Key) ?? false)
                                                        && (drawings?.ContainsKey(x.Key) ?? false)
                                                        && (drawings[x.Key].IsPublic
                                                        && !drawingInfo[x.Key].IsBanned
                                                        || (includePrivates && drawings[x.Key].Owner == userId && !drawingInfo[x.Key].IsBanned)))
                                            .Select(x => x.Key).ToList();

            drawingIds?.Sort((a, b) => drawingInfo[b].LastModifiedOn.CompareTo(drawingInfo[a].LastModifiedOn));

            return drawingIds ?? new List<string>();
        }

        public async Task<ObservableCollection<string>> GetDrawingIdTaggedAs(string tag)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting drawings tagged as ${tag} while logged out.");
                return null;
            }

            Logger.Info($"Getting drawings tagged as ${tag} for user ${AuthService.CurrentUser.DisplayName}");

            if (string.IsNullOrEmpty(tag) || tag.Length < Constants.MinSearchLetter)
                return new ObservableCollection<string>();

            var infos = await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                              .Once<Dictionary<string, DrawingInfo>>();

            var selectedDrawings = new ObservableCollection<string>();
            foreach (var info in infos)
            {
                if (info.Value.Tags == null || info.Value.IsBanned)
                    continue;

                foreach (string infoTag in info.Value.Tags)
                {
                    if (infoTag.ToLower().Contains(tag.ToLower()))
                    {
                        selectedDrawings.Add(info.Key);
                        break;
                    }
                }
            }

            return selectedDrawings;
        }
        public async Task TagDrawingInfoAsEmailSent(string emailSent)
        {
            await DatabaseService.Ref(DatabasePaths.DrawingInfo)
                                 .Child(emailSent)
                                 .Child(DatabasePaths.EmailSent)
                                 .Set<bool>(true);
        }

        public async Task DeleteDrawing(string id)
        {
            await DatabaseService.Ref(DatabasePaths.Drawings).Child(id).Remove();
            await DatabaseService.Ref(DatabasePaths.DrawingInfo).Child(id).Remove();
            await DatabaseService.Ref(DatabasePaths.Strokes).Child(id).Remove();
            var users = await DatabaseService.Ref(DatabasePaths.Users).Once<Dictionary<string, object>>();
            foreach (var userId in users.Keys)
            {
                await DatabaseService.Ref(DatabasePaths.Users).Child(userId).Child(DatabasePaths.Drawings).Child(id).Remove();
            }
        }

        public async Task AddDrawingToUser(string userId, string drawingId)
        {
            await DatabaseService.Ref(DatabasePaths.Users)
                                 .Child(userId).Child(DatabasePaths.Drawings)
                                 .Child(drawingId)
                                 .Set<string>(drawingId);
        }

        private async Task CleanSaucedDrawings()
        {
            var drawingInfo = await DatabaseService.Ref(DatabasePaths.DrawingInfo).Once<Dictionary<string, DrawingInfo>>();
            var drawings = await DatabaseService.Ref(DatabasePaths.Drawings).Once<Dictionary<string, DrawingModel>>();

            var toClear = new List<string>();

            foreach (var key in drawingInfo.Keys.Concat(drawings.Keys))
            {
                if (!drawings.ContainsKey(key) || !drawingInfo.ContainsKey(key))
                {
                    toClear.Add(key);
                }
            }

            var users = await DatabaseService.Ref(DatabasePaths.Users).Once<Dictionary<string, UserModel>>();

            foreach (var key in toClear)
            {
                foreach (var user in users.Values)
                {
                    if (user?.Drawings?.ContainsKey(key) ?? false)
                    {
                        user.Drawings.Remove(key);
                    }
                }

                await DatabaseService.Ref(DatabasePaths.DrawingInfo).Child(key).Remove();
                await DatabaseService.Ref(DatabasePaths.Drawings).Child(key).Remove();
                await DatabaseService.Ref(DatabasePaths.Strokes).Child(key).Remove();
                await DatabaseService.Ref(DatabasePaths.SelectedStrokes).Child(key).Remove();
            }

            await DatabaseService.Ref(DatabasePaths.Users).Set(users);
        }

        private static class Constants
        {
            public const int DefaultDrawingWidth = 500;
            public const int DefaultDrawingHeight = 300;
            public const int MinSearchLetter = 3;
        }
    }
}