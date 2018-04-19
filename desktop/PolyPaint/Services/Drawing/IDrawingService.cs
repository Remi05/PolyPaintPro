using PolyPaint.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace PolyPaint.Services.Drawing
{
    public interface IDrawingService
    {
        Task<Drawing> CreateDrawing(bool isPublic, bool isProtected, SecureString password);
        Task<Drawing> GetDrawing(string drawingId);
        Task<List<string>> GetPublicDrawingsIds(bool includeProtected);
        Task<List<string>> GetDrawingsIds(string userId, bool includePrivates);
        Task<DrawingInfo> GetDrawingInfo(string drawingId);
        Task SaveDrawingPreview(string drawingId, Stream drawingStream, Stream thumbnailStream);
        Task SetIsDrawingLiked(string drawingId, bool isLiked);
        Task SetIsDrawingNsfw(string drawingId, bool isNsfw);
        Task Report(string drawingId, string reason);
        Task UndoReport(string drawingId);
        Task<ObservableCollection<string>> GetDrawingIdTaggedAs(string tag);
        Task TagDrawingInfoAsEmailSent(string drawingId);
        Task DeleteDrawing(string id);
        Task AddDrawingToUser(string userId, string drawingId);
    }
}