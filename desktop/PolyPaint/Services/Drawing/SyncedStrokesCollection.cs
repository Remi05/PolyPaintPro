using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Cache;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyPaint.Services.Drawing
{
    using StrokeModelsMap = ConcurrentDictionary<string, StrokeModel>;
    using StrokeModelsList = List<StrokeModel>;

    class SyncedStrokeCollection
    {
        private ILogger Logger { get; }
        private ICacheService Cache { get; }
        private IDatabaseService DatabaseService { get; }
        private IAuthenticationService AuthService { get; }

        private string DrawingId { get; }
        private bool IsOffline => AuthService.CurrentUser == null;

        private StrokeModelsMap StrokesMap { get; set; } = new StrokeModelsMap();
        private StrokeModelsMap StrokesToUpdate { get; set; } = new StrokeModelsMap();
        private StrokeModelsMap StrokesMapOverride { get; set; } = new StrokeModelsMap();

        private CancellationTokenSource CancellationTokenSource { get; set; }
        private ISubscription StrokesSubscription { get; set; }
        private object StrokesLock { get; set; } = new object();

        public event Action StrokesUpdated;

        public SyncedStrokeCollection(string drawingId, IAuthenticationService authService, IDatabaseService databaseService, ICacheService cache, ILogger logger)
        {
            DrawingId = drawingId;

            DatabaseService = databaseService;
            AuthService = authService;
            Logger = logger;
            Cache = cache;

            StartOnlineSync();
        }

        public async void StartOnlineSync()
        {
            await PullStrokes();

            // Fire and forget threads
            CancellationTokenSource = new CancellationTokenSource();

            foreach (var stroke in StrokesToUpdate)
            {
                stroke.Value.AuthorId = AuthService.CurrentUser.Id;
            }

            if (!IsOffline)
            {
                var strokesRef = DatabaseService.Ref(DatabasePaths.Strokes).Child(DrawingId);
                StrokesSubscription = strokesRef.OnValue<StrokeModelsMap>(SyncRemoteStrokesToLocal);
            }

#pragma warning disable 
            Task.Run(() => StrokesWriterThread(CancellationTokenSource.Token));
#pragma warning restore
        }

        public void StopSync()
        {
            CancellationTokenSource?.Cancel();
        }

        private async Task StrokesWriterThread(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Info("The strokes writer thread was cancelled.");
                    return;
                }

                Thread.Sleep(IsOffline ? Constants.OfflineRefreshRate : Constants.OnlineRefreshRate);
                if (StrokesToUpdate.Count == 0) { continue; }

                StrokeModelsMap strokesSnapshot;
                lock (StrokesLock)
                {
                    strokesSnapshot = new StrokeModelsMap(StrokesToUpdate);
                    StrokesToUpdate.Clear();
                }

                if (IsOffline)
                {
                    Cache.Write(DatabasePaths.Strokes, DrawingId, StrokesMap);
                }
                else
                {
                    await DatabaseService.Ref(DatabasePaths.Strokes).Child(DrawingId).Update(strokesSnapshot);
                }
            }
        }

        private async Task PullStrokes()
        {
            if (IsOffline)
            {
                StrokesMap = Cache.Get<ConcurrentDictionary<string, StrokeModel>>(DatabasePaths.Strokes, DrawingId) ?? new StrokeModelsMap();
#pragma warning disable CS4014
                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    StrokesUpdated?.Invoke();
                });
#pragma warning restore CS4014
            }
            else
            {
                StrokesMap = await DatabaseService.Ref(DatabasePaths.Strokes).Child(DrawingId).Once<StrokeModelsMap>();
            }
        }

        public void UpdateStroke(string id, StrokeModel stroke)
        {
            if (stroke != null)
            {
                stroke.AuthorId = AuthService.CurrentUser?.Id ?? AuthService.OfflineClientId;
                stroke.LastModificationDate = DateTime.Now;
                Logger.Debug($"Adding stroke ${id}");
                if (IsOffline)
                {
                    StrokesMap[id] = stroke;
                }
            }
            else
            {
                Logger.Debug($"Removing stroke ${id}");
                if (IsOffline)
                {
                    StrokeModel _;
                    StrokesMap.TryRemove(id, out _);
                }
            }

            StrokesToUpdate[id] = stroke;
            StrokesMapOverride[id] = stroke;

            if (IsOffline)
            {
                StrokesUpdated?.Invoke();
            }
        }

        private void SyncRemoteStrokesToLocal(StrokeModelsMap strokes)
        {
            StrokesMap = strokes;
            PurgeOverrideMap();
            StrokesUpdated?.Invoke();
        }

        private void PurgeOverrideMap()
        {
            StrokeModel _;
            foreach (var stroke in StrokesMapOverride)
            {
                if (stroke.Value != null && StrokesMap.ContainsKey(stroke.Key))
                {
                    StrokeModel currentStroke;
                    StrokesMap.TryGetValue(stroke.Value.Id, out currentStroke);
                    if (currentStroke.LastModificationDate >= stroke.Value.LastModificationDate)
                    {
                        StrokesMapOverride.TryRemove(stroke.Key, out _);
                    }
                }
                else if (stroke.Value == null && !StrokesMap.ContainsKey(stroke.Key))
                {
                    StrokesMapOverride.TryRemove(stroke.Key, out _);
                }
            }
        }

        public StrokeModel GetStroke(string id)
        {
            StrokeModel stroke;
            StrokesMap.TryGetValue(id, out stroke);
            return stroke;
        }

        public IEnumerable<StrokeModel> GetStrokes(IEnumerable<string> ids)
        {
            var strokes = new List<StrokeModel>();
            foreach (var id in ids)
            {
                var stroke = GetStroke(id);
                if (stroke != null)
                {
                    strokes.Add(stroke);
                }
            }

            return strokes;
        }

        /// <summary>
        /// Merges the strokes map with the strokes map override to give the most recent strokes list available.
        /// </summary>
        public StrokeModelsList GetMergedStrokeMaps()
        {
            StrokeModel _;
            foreach (var stroke in StrokesMapOverride)
            {
                if (stroke.Value == null) { StrokesMap.TryRemove(stroke.Key, out _); }
                else { StrokesMap[stroke.Key] = stroke.Value; }
            }

            var strokes = StrokesMap.Select(x => x.Value).ToList();
            strokes.Sort((x, y) => DateTime.Compare(x.CreatedDate, y.CreatedDate));
            return strokes;
        }

        ~SyncedStrokeCollection()
        {
            if (StrokesSubscription != null)
            {
                StrokesSubscription.Stop();
            }
        }

        private static class Constants
        {
            public const int OnlineRefreshRate = 1;
            public const int OfflineRefreshRate = 1000;
        }
    }
}
