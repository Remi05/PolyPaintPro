using PolyPaint.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PolyPaint.Services
{
    public class ExportService : IExportService
    {
        public void Export(InkCanvas canvas, ImageFormat media)
        {
            Stream exportFileStream;
            SaveFileDialog fileDialog = new SaveFileDialog
            {
                Filter = media == ImageFormat.Png ? "Files | *.png;" : "Files | *.jpg; *.jpeg;"
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                using (exportFileStream = fileDialog.OpenFile())
                {
                    ExportWithoutSaving(canvas, media, exportFileStream);
                    exportFileStream.Close();
                }
            }
        }

        public void ExportWithoutSaving(InkCanvas canvas, ImageFormat media, Stream previewStream, Stream thumbnailStream = null)
        {
            Rect bounds = new Rect(0, 0, canvas.ActualWidth, canvas.ActualHeight);
            double dpi = 96d;
            var rederTargetBitmap = new RenderTargetBitmap(
                                                            (int)bounds.Width,
                                                            (int)bounds.Height,
                                                            dpi, dpi,
                                                            PixelFormats.Default);
            var drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(canvas);
                drawingContext.DrawRectangle(visualBrush, null, new Rect(new System.Windows.Point(), bounds.Size));
            }

            rederTargetBitmap.Render(drawingVisual);
            var encoder = media == ImageFormat.Png ? new PngBitmapEncoder() as BitmapEncoder : new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rederTargetBitmap));

            encoder.Save(previewStream);

            if (thumbnailStream != null)
            {
                using (var stream = new MemoryStream())
                {
                    var image = System.Drawing.Image.FromStream(previewStream);
                    previewStream.Seek(0, SeekOrigin.Begin);
                    var thumbnailHeight = Constants.ThumbnailHeight;
                    var thumbnailWidth = thumbnailHeight * image.Width / image.Height;
                    var thumbnail = image.GetThumbnailImage(thumbnailWidth, thumbnailHeight, () => false, IntPtr.Zero);
                    thumbnail.ToStream(media).CopyTo(thumbnailStream);
                }
            }
        }

        private static class Constants
        {
            public static readonly int ThumbnailHeight = 128;
        }
    }
}
