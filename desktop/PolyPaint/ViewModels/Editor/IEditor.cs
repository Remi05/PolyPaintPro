using PolyPaint.Services;
using PolyPaint.ViewModels;
using System.Drawing.Imaging;
using System.Windows.Controls;

namespace PolyPaint.Models
{
    public interface IEditor : IViewModel
    {
        string SelectedColor { get; set; }
        string SelectedTool { get; set; }
        string SelectedStyle { get; set; }
        int StrokeSize { get; set; }

        void SelectTool(string outil);
        void SelectStyle(string pointe);
        void ExportPng(InkCanvas canvas);
        void ExportJpg(InkCanvas canvas);
        void SaveToGoogleDrive(InkCanvas canvas, ImageFormat format);
        void ShareOnFacebook(InkCanvas canvas);
        void SaveDrawingPreview(string drawingId, InkCanvas canvas);
    }
}