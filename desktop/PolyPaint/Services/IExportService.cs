using System.Drawing.Imaging;
using System.IO;
using System.Windows.Controls;

namespace PolyPaint.Services
{
    public interface IExportService
    {
        void Export(InkCanvas canvas, ImageFormat media);
        void ExportWithoutSaving(InkCanvas canvas, ImageFormat media, Stream previewStream, Stream thumbnailStream = null);
    }
}