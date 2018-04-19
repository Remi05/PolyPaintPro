using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Cache;
using PolyPaint.Utils;
using PolyPaint.Extensions;

namespace PolyPaint.Services.Drawing
{
    class LocalDrawingService : IDrawingService
    {
        private ILogger Logger { get; }
        private ICacheService Cache { get; }
        private IDatabaseService Database { get; }
        private IAuthenticationService AuthService { get; }

        public LocalDrawingService(ICacheService cache, IAuthenticationService authService, IDatabaseService database, ILogger logger)
        {
            Cache = cache;
            Logger = logger;
            Database = database;
            AuthService = authService;
        }

        public Task<Drawing> CreateDrawing(bool isPublic, bool isProtected, SecureString password)
        {
            var id = Guid.NewGuid().ToString();

            var drawingModel = new DrawingModel()
            {
                Height = Constants.DefaultDrawingHeight,
                Width = Constants.DefaultDrawingWidth,
                Owner = AuthService.OfflineClientId,
                IsProtected = isProtected,
                IsPublic = isPublic,
                Password = password?.ToUnsecureString()
            };

            var drawingInfo = new DrawingInfo()
            {
                Id = id,
                LastModifiedOn = DateTime.Now,
                Owner = AuthService.OfflineClientId,
                PreviewUrl = id,
                ThumbnailUrl = id + "_thumbnail"
            };

            var drawing = new Drawing(id, drawingModel, AuthService, Cache, Database, Logger);

            Cache.Write(DatabasePaths.Drawings, drawing.Id, drawingModel);
            Cache.Write(DatabasePaths.DrawingInfo, drawing.Id, drawingInfo);

            return Task.Run(() => drawing);
        }

        public Task<Drawing> GetDrawing(string drawingId)
        {
            var drawingModel = Cache.Get<DrawingModel>(DatabasePaths.Drawings, drawingId);
            return Task.Run(() => new Drawing(drawingId, drawingModel, AuthService, Cache, Database, Logger));
        }

        public Task<ObservableCollection<string>> GetDrawingIdTaggedAs(string tag)
        {
            Logger.Debug("Method GetDrawingIdTaggedAs(string tag) is not implemented in offline mode.");
            return Task.Run(() => new ObservableCollection<string>());
        }

        public Task<DrawingInfo> GetDrawingInfo(string drawingId)
        {
            return Task.Run(() => Cache.Get<DrawingInfo>(DatabasePaths.DrawingInfo, drawingId));
        }

        public Task<List<string>> GetDrawingsIds(string userId, bool includePrivates)
        {
            var user = Cache.Get<UserModel>(DatabasePaths.Users, userId);
            return Task.Run(() => user?.Drawings?.Keys.ToList() ?? new List<string>());
        }

        public Task<List<string>> GetPublicDrawingsIds(bool includeProtected)
        {
            return Task.Run(() => Cache.GetAllIds(DatabasePaths.Drawings)
                                       .Where(x => Cache.Get<DrawingModel>(DatabasePaths.Drawings, x).IsPublic && (includeProtected || !Cache.Get<DrawingModel>(DatabasePaths.Drawings, x).IsProtected))
                                       .ToList());
        }

        public Task Report(string drawingId, string reason)
        {
            Logger.Debug("Method Report(string drawingId, string reason) is not implemented in offline mode.");
            return Task.Run(() => { });
        }

        public Task SaveDrawingPreview(string drawingId, Stream drawingStream, Stream thumbnailStream)
        {
            var info = Cache.Get<DrawingInfo>(DatabasePaths.DrawingInfo, drawingId);

            if (info == null)
            {
                Logger.Warn($"No drawing info for drawing {drawingId}. Can't save preview.");
                return Task.Run(() => { });
            }

            info.LastModifiedOn = DateTime.Now;
            Cache.UpdateResource(info.PreviewUrl, drawingStream);
            Cache.UpdateResource(info.ThumbnailUrl, thumbnailStream);
            Cache.Write(DatabasePaths.DrawingInfo, drawingId, info);
            return Task.Run(() => { });
        }

        public Task SetIsDrawingLiked(string drawingId, bool isLiked)
        {
            Logger.Debug("Method SetIsDrawingLiked(string drawingId, bool isLiked) is not implemented in offline mode.");
            return Task.Run(() => { });
        }

        public Task SetIsDrawingNsfw(string drawingId, bool isNsfw)
        {
            Logger.Debug("Method SetIsDrawingNsfw(string drawingId, bool isNsfw) is not implemented in offline mode.");
            return Task.Run(() => { });
        }

        public Task UndoReport(string drawingId)
        {
            Logger.Debug("Method UndoReport(string drawingId) is not implemented in offline mode.");
            return Task.Run(() => { });
        }

        public Task DeleteDrawing(string id)
        {
            Logger.Debug("Method DeleteDrawing(string id) is not implemented in offline mode.");
            return Task.Run(() => { });
        }

        public Task TagDrawingInfoAsEmailSent(string drawingId)
        {
            return Task.Run(() => { });
        }

        public Task AddDrawingToUser(string userId, string drawingId)
        {
            var user = Cache.Get<UserModel>(DatabasePaths.Users, userId);
            user.Drawings[drawingId] = drawingId;
            Cache.Write(DatabasePaths.Users, userId, user);
            return Task.Run(() => { });
        }

        private static class Constants
        {
            public const int DefaultDrawingWidth = 500;
            public const int DefaultDrawingHeight = 300;
        }
    }
}
