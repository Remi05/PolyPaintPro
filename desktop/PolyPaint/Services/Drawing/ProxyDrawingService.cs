using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Auth;

namespace PolyPaint.Services.Drawing
{
    class ProxyDrawingService : IDrawingService
    {
        IAuthenticationService AuthService { get; }
        DrawingService OnlineDrawingService { get; }
        LocalDrawingService LocalDrawingService { get; }


        IDrawingService DrawingService => AuthService.IsLoggedIn
                      ? OnlineDrawingService as IDrawingService
                      : LocalDrawingService as IDrawingService;

        public ProxyDrawingService(IAuthenticationService authService, DrawingService onlineDrawingService, LocalDrawingService localDrawingService)
        {
            AuthService = authService;
            LocalDrawingService = localDrawingService;
            OnlineDrawingService = onlineDrawingService;
        }

        public Task<Drawing> CreateDrawing(bool isPublic, bool isProtected, SecureString password)
        {
            return DrawingService.CreateDrawing(isPublic, isProtected, password);
        }

        public Task<Drawing> GetDrawing(string drawingId)
        {
            return DrawingService.GetDrawing(drawingId);
        }

        public Task<List<string>> GetPublicDrawingsIds(bool includeProtected)
        {
            return DrawingService.GetPublicDrawingsIds(includeProtected);
        }

        public Task<List<string>> GetDrawingsIds(string userId, bool includePrivates)
        {
            return DrawingService.GetDrawingsIds(userId, includePrivates);
        }

        public Task<DrawingInfo> GetDrawingInfo(string drawingId)
        {
            return DrawingService.GetDrawingInfo(drawingId);
        }

        public async Task SaveDrawingPreview(string drawingId, Stream drawingStream, Stream thumbnailStream)
        {
            await DrawingService.SaveDrawingPreview(drawingId, drawingStream, thumbnailStream);
        }

        public async Task SetIsDrawingLiked(string drawingId, bool isLiked)
        {
            await DrawingService.SetIsDrawingLiked(drawingId, isLiked);
        }

        public async Task SetIsDrawingNsfw(string drawingId, bool isNsfw)
        {
            await DrawingService.SetIsDrawingNsfw(drawingId, isNsfw);
        }

        public async Task Report(string drawingId, string reason)
        {
            await DrawingService.Report(drawingId, reason);
        }

        public async Task UndoReport(string drawingId)
        {
            await DrawingService.UndoReport(drawingId);
        }

        public Task<ObservableCollection<string>> GetDrawingIdTaggedAs(string tag)
        {
            return DrawingService.GetDrawingIdTaggedAs(tag);
        }

        public async Task DeleteDrawing(string id)
        {
            await DrawingService.DeleteDrawing(id);
        }

        public async Task TagDrawingInfoAsEmailSent(string drawingId)
        {
            await DrawingService.TagDrawingInfoAsEmailSent(drawingId);
        }

        public async Task AddDrawingToUser(string userId, string drawingId)
        {
            await DrawingService.AddDrawingToUser(userId, drawingId);
        }
    }
}
