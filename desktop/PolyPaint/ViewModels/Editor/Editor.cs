using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PolyPaint.Services;
using PolyPaint.Services.Achievements;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Toasts;
using PolyPaint.Utils;
using PolyPaint.ViewModels;
using PolyPaint.Views;

namespace PolyPaint.Models
{
    class Editor : ViewModel, IEditor
    {
        private IAuthenticationService AuthService { get; }
        private IDrawingService DrawingService { get; }
        private IAchievementsService AchievementsService { get; }
        private IToastsService ToastsService { get; }
        private IExportService ExportService { get; }
        private BrowserView BrowserWindow { get; set; }
        private InkCanvas Canvas { get; set; }
        private FacebookCaptionView CaptionWindow { get; set; }

        private string selectedTool = "pencil";
        public string SelectedTool
        {
            get { return selectedTool; }
            set { selectedTool = value; RaisePropertyChanged(); }
        }

        // Pencil style (round, horizontal, vertical, etc.)
        private string selectedStyle = "ronde";
        public string SelectedStyle
        {
            get { return selectedStyle; }
            set
            {
                SelectedTool = "pencil";
                selectedStyle = value;
                RaisePropertyChanged();
            }
        }

        private string selectedColor = "Black";
        public string SelectedColor
        {
            get { return selectedColor; }
            set
            {
                selectedColor = value;
                SelectedTool = "pencil"; // The user probably wants to use the pencil tool since he or she changed the pencil color
                RaisePropertyChanged();
            }
        }

        private int strokeSize = 11;
        public int StrokeSize
        {
            get { return strokeSize; }
            set
            {
                strokeSize = value;
                SelectedTool = "pencil";  // The user probably wants to use the pencil tool since he or she changed the pencil size
                RaisePropertyChanged();
            }
        }

        public Editor(IAuthenticationService authService,
                      IDrawingService drawingService,
                      IAchievementsService achievementsService,
                      IToastsService toastsService,
                      IExportService exportService)
        {
            AuthService = authService;
            DrawingService = drawingService;
            AchievementsService = achievementsService;
            ToastsService = toastsService;
            ExportService = exportService;
        }

        public void SelectStyle(string style) => SelectedStyle = style;

        public void SelectTool(string tool) => SelectedTool = tool;

        public void ExportPng(InkCanvas canvas)
        {
            ExportService.Export(canvas, ImageFormat.Png);
        }

        public void ExportJpg(InkCanvas canvas)
        {
            ExportService.Export(canvas, ImageFormat.Jpeg);
        }

        public async void SaveDrawingPreview(string drawingId, InkCanvas canvas)
        {
            using (var previewStream = new MemoryStream())
            using (var thumbnailStream = new MemoryStream())
            {
                ExportService.ExportWithoutSaving(canvas, ImageFormat.Jpeg, previewStream, thumbnailStream);
                previewStream.Seek(0, SeekOrigin.Begin);
                thumbnailStream.Seek(0, SeekOrigin.Begin);

                await DrawingService.SaveDrawingPreview(drawingId, previewStream, thumbnailStream);
            }
        }

        public async void SaveToGoogleDrive(InkCanvas canvas, ImageFormat format)
        {
            Canvas = canvas;
            if (string.IsNullOrWhiteSpace(AuthService.GoogleAccessToken))
            {
                if (BrowserWindow?.IsVisible ?? false)
                    return;

                BrowserWindow = new BrowserView(GoogleAPI.GoogleAuthenticationUri);

                BrowserWindow.GoogleConnected += Browser_GoogleConnected;
                BrowserWindow.Show();
                return;
            }
            await SendToGoogle(format);
        }

        private async Task SendToGoogle(ImageFormat format)
        {
            using (var imageStream = new MemoryStream())
            {
                ExportService.ExportWithoutSaving(Canvas, format, imageStream);
                string feedback = await GoogleAPI.SaveOnGoogleDrive(imageStream, ".png", AuthService.GoogleAccessToken);
                ToastsService.Pop("Google Drive Sharing", feedback, Constants.DriveIconUri);
                if (AuthService.IsLoggedIn)
                {
                    await AchievementsService.Increment(AuthService.CurrentUser.Id, AchievementMetrics.SharesOnDrive);
                }                    
            }
        }

        private async void Browser_GoogleConnected(object sender, EventArgs e)
        {
            BrowserWindow.Close();
            var args = e as ConnectedEventArgs;
            AuthService.GoogleAccessToken = args.ConnectionToken;
            await SendToGoogle(ImageFormat.Png);
        }


        public void ShareOnFacebook(InkCanvas canvas)
        {
            Canvas = canvas;
            var imageStream = new MemoryStream();
            ExportService.ExportWithoutSaving(canvas, ImageFormat.Jpeg, imageStream);

            if (string.IsNullOrWhiteSpace(AuthService.FacebookAccessToken))
            {
                if (BrowserWindow?.IsVisible ?? false)
                    return;

                BrowserWindow = new BrowserView(FacebookAPI.FacebookAuthenticationUri);
                BrowserWindow.FacebookConnected += (s, e) =>
                {
                    var args = e as ConnectedEventArgs;
                    AuthService.FacebookAccessToken = args.ConnectionToken;
                    BrowserWindow.Close();
                    GetFacebookCaption(imageStream);
                };

                BrowserWindow.Show();
                return;
            }

            GetFacebookCaption(imageStream);
        }
        
        private void GetFacebookCaption(MemoryStream image)
        {
            if (CaptionWindow?.IsVisible ?? false)
                return;
            CaptionWindow = new FacebookCaptionView();
            CaptionWindow.CaptionWritten += async (caption) =>
            {
                string feedback = await FacebookAPI.ShareImage(image, AuthService.FacebookAccessToken, caption);
                ToastsService.Pop("Facebook sharing", feedback, Constants.FacebookIconUri);
                if (AuthService.IsLoggedIn)
                {
                    await AchievementsService.Increment(AuthService.CurrentUser.Id, AchievementMetrics.SharesOnFacebook);
                }
            };

            CaptionWindow.Show();
        }

        private static class Constants
        {
            public readonly static string FacebookIconUri = "pack://application:,,,/Resources/facebook-flat-circular.png";
            public readonly static string DriveIconUri = "pack://application:,,,/Resources/drive-flat-circular.png";
        }
    }
}