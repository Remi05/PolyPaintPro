using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using PolyPaint.Extensions;
using PolyPaint.Models;
using PolyPaint.Services;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Toasts;
using PolyPaint.Views.Drawing;

namespace PolyPaint.ViewModels
{
    public interface IDrawingEditingViewModel : IViewModel
    {
        event Action GoBackClicked;
        event Action WasKickedOut;
        event Action<StrokeCollection> SelectionChanged;

        InkCanvas DrawingCanvas { get; set; }
        DrawingAttributes DrawingAttributes { get; set; }
        StrokeCollection Strokes { get; }
        double Width { get; }
        double Height { get; }
        string SelectedColor { get; }
        string SelectedStyle { get; }
        string SelectedTool { get; }
        int StrokeSize { get; }
        bool IsLoggedIn { get; }
        bool IsNsfw { get; }
        bool IsOwner { get; }

        void SetDrawing(Services.Drawing.Drawing drawing);
        void SaveDrawingPreview();
    }

    public class DrawingEditingViewModel : ViewModel, IDrawingEditingViewModel
    {
        public event Action GoBackClicked;
        public event Action WasKickedOut;
        public event Action<StrokeCollection> SelectionChanged;

        private IAuthenticationService AuthService { get; }
        private IDrawingService DrawingService { get; }
        private IViewsManager ViewsManager { get; }
        private IToastsService ToastsService { get; }
        public IEditor Editor { get; }

        public bool IsOwner => Drawing != null && Drawing.DrawingModel?.Owner == AuthService?.CurrentUser?.Id;

        private Services.Drawing.Drawing drawing;
        private Services.Drawing.Drawing Drawing
        {
            get => drawing;
            set
            {
                drawing = value;
                drawing.SizeChanged += (_, __) =>
                {
                    RaisePropertyChanged(nameof(Width));
                    RaisePropertyChanged(nameof(Height));
                };
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Strokes));
                RaisePropertyChanged(nameof(DrawingAttributes));
            }
        }

        private DrawingInfo drawingInfo;
        private DrawingInfo DrawingInfo
        {
            get => drawingInfo;
            set
            {
                drawingInfo = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsNsfw));
            }
        }

        public InkCanvas DrawingCanvas { get; set; }
        public DrawingAttributes DrawingAttributes { get; set; } = new DrawingAttributes();
        public StrokeCollection Strokes => Drawing?.Strokes ?? new StrokeCollection();

        public double Width
        {
            get => Drawing?.DrawingModel?.Width ?? 0;
            set
            {
                if (Drawing != null && Drawing.DrawingModel != null)
                {
                    Drawing.DrawingModel.Width = (int)value;
                }
                RaisePropertyChanged();
            }
        }

        public double Height
        {
            get => Drawing?.DrawingModel?.Height ?? 0;
            set
            {
                if (Drawing != null && Drawing.DrawingModel != null)
                {
                    Drawing.DrawingModel.Height = (int)value;
                }
                RaisePropertyChanged();
            }
        }

        public string SelectedColor
        {
            get => Editor.SelectedColor;
            set => Editor.SelectedColor = value;
        }

        public string SelectedStyle
        {
            get => Editor.SelectedStyle;
            set => RaisePropertyChanged();
        }

        public string SelectedTool
        {
            get => Editor.SelectedTool;
            set => RaisePropertyChanged();
        }

        public int StrokeSize
        {
            get => Editor.StrokeSize;
            set => Editor.StrokeSize = value;
        }

        private bool isLoggedIn;
        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set { isLoggedIn = value; RaisePropertyChanged(); }
        }

        public bool IsNsfw => DrawingInfo?.IsNsfw ?? false;

        public RelayCommand ConfigureCommand { get; }
        public RelayCommand GoBackCommand { get; }
        public RelayCommand<InkCanvas> ExportCommand { get; }
        public RelayCommand<InkCanvas> SaveOnGoogleDriveCommand { get; }
        public RelayCommand<InkCanvas> ShareOnFacebookCommand { get; }
        public RelayCommand ToggleNsfwCommand { get; }
        public RelayCommand CutCommand { get; set; }
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }
        public RelayCommand DuplicateCommand { get; set; }
        public RelayCommand<string> SelectToolCommand { get; set; }
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand SaveDimensionsCommand { get; set; }
        public RelayCommand SelectionTransformedCommand { get; set; }
        public RelayCommand<InkCanvasStrokeErasingEventArgs> StrokeErasingCommand { get; set; }
        public RelayCommand<InkCanvasSelectionChangingEventArgs> SelectionChangingCommand { get; set; }
        public RelayCommand<RoutedEventArgs> EditingModeChangedCommand { get; set; }

        public DrawingEditingViewModel(
            IAuthenticationService authService,
            IDrawingService drawingService,
            IViewsManager viewsManager,
            IToastsService toastsService,
            IEditor editor
        )
        {
            AuthService = authService;
            DrawingService = drawingService;
            ViewsManager = viewsManager;
            ToastsService = toastsService;
            Editor = editor;

            Width = Constants.DefaultCanvasWidth;
            Height = Constants.DefaultCanvasHeight;

            Editor.PropertyChanged += EditorPropertyChanged;

            DrawingAttributes = new DrawingAttributes();
            DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(Editor.SelectedColor);
            AdjustStyle();

            ConfigureCommand = new RelayCommand(PopConfigurationWindow);
            GoBackCommand = new RelayCommand(GoBack);
            ExportCommand = new RelayCommand<InkCanvas>(Editor.ExportPng);
            SaveOnGoogleDriveCommand = new RelayCommand<InkCanvas>((canvas) => Editor.SaveToGoogleDrive(canvas, ImageFormat.Png));
            ShareOnFacebookCommand = new RelayCommand<InkCanvas>(Editor.ShareOnFacebook);
            ToggleNsfwCommand = new RelayCommand(ToggleNsfw);
            SelectToolCommand = new RelayCommand<string>(Editor.SelectTool);
        }

        public async void SetDrawing(Services.Drawing.Drawing drawing)
        {
            Drawing = drawing;

            if (Drawing == null)
                return;

            DrawingInfo = await DrawingService.GetDrawingInfo(Drawing.Id);
            Drawing.SelectionChanged += strokes => SelectionChanged?.Invoke(new StrokeCollection(strokes));

            BindDrawingCommands();
            BindEvents();
        }

        public void SaveDrawingPreview()
        {
            if (Drawing == null || DrawingCanvas == null)
                return;

            DrawingCanvas.Select(null, null);
            Editor.SaveDrawingPreview(Drawing.Id, DrawingCanvas);
        }

        private async void GoBack()
        {
            await Drawing.Stop();
            GoBackClicked?.Invoke();
        }

        public async void ToggleNsfw()
        {
            if (Drawing == null)
                return;

            var newIsNsfwValue = !IsNsfw;
            // Mock the update for it to be more responsive.
            DrawingInfo.IsNsfw = newIsNsfwValue;
            RaisePropertyChanged(nameof(IsNsfw));

            await DrawingService.SetIsDrawingNsfw(Drawing.Id, newIsNsfwValue);
            DrawingInfo = await DrawingService.GetDrawingInfo(Drawing.Id);
        }

        private void EditorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedColor")
            {
                DrawingAttributes.Color = (Color)ColorConverter.ConvertFromString(Editor.SelectedColor);
            }
            else if (e.PropertyName == "SelectedTool")
            {
                SelectedTool = Editor.SelectedTool;
            }
            else if (e.PropertyName == "SelectedStyle")
            {
                SelectedStyle = Editor.SelectedStyle;
                AdjustStyle();
            }
            else // TODO: add if(e.PropertyName == "StrokeSize")
            {
                AdjustStyle();
            }
        }

        private void AdjustStyle()
        {
            DrawingAttributes.StylusTip = Editor.SelectedStyle == "ronde" ? StylusTip.Ellipse : StylusTip.Rectangle;
            DrawingAttributes.Width = Editor.SelectedStyle == "verticale" ? 1 : Editor.StrokeSize;
            DrawingAttributes.Height = Editor.SelectedStyle == "horizontale" ? 1 : Editor.StrokeSize;
        }

        private void BindDrawingCommands()
        {
            CutCommand = new RelayCommand(() => Drawing?.Cut());
            UndoCommand = new RelayCommand(() => Drawing?.Undo());
            RedoCommand = new RelayCommand(() => Drawing?.Redo());
            ResetCommand = new RelayCommand(() => Drawing?.Reset());
            DuplicateCommand = new RelayCommand(() => Drawing?.Duplicate());

            SaveDimensionsCommand = new RelayCommand(() => Drawing?.SaveDimensions());
            SelectionChangingCommand = new RelayCommand<InkCanvasSelectionChangingEventArgs>(e => Drawing?.OnSelectionChanged(e));
            SelectionTransformedCommand = new RelayCommand(() => Drawing?.OnSelectionTransformed());

            StrokeErasingCommand = new RelayCommand<InkCanvasStrokeErasingEventArgs>(e => Drawing?.OnStrokeErased(e));
            EditingModeChangedCommand = new RelayCommand<RoutedEventArgs>(e =>
            {
                if (Drawing == null)
                    return;

                Drawing.EditingMode = (e.Source as InkCanvas)?.EditingMode;
            });

            RaisePropertyChanged(nameof(CutCommand));
            RaisePropertyChanged(nameof(UndoCommand));
            RaisePropertyChanged(nameof(RedoCommand));
            RaisePropertyChanged(nameof(ResetCommand));
            RaisePropertyChanged(nameof(DuplicateCommand));
            RaisePropertyChanged(nameof(SaveDimensionsCommand));
            RaisePropertyChanged(nameof(SelectionChangingCommand));
            RaisePropertyChanged(nameof(SelectionTransformedCommand));
            RaisePropertyChanged(nameof(StrokeErasingCommand));
            RaisePropertyChanged(nameof(EditingModeChangedCommand));
            RaisePropertyChanged(nameof(IsOwner));
        }

        private void BindEvents()
        {
            Drawing.KickedOut += () =>
            {
                ToastsService.Pop("Woops!", "This drawing is now private, you can not see it anymore.", Constants.PadlockUri);
                WasKickedOut?.Invoke();
            };

            Drawing.PasswordRequested += () =>
            {
                ToastsService.Pop("Woops!", "This drawing is now protected. If you know the password, you can still edit it.", Constants.PadlockUri);
                WasKickedOut?.Invoke();
            };
        }

        private async void PopConfigurationWindow()
        {
            var configurationWindow = ViewsManager.Get<DrawingConfigurationWizard>();
            configurationWindow.ViewModel.SetDrawing(Drawing?.DrawingModel, isNewDrawing: false);
            if (configurationWindow.ShowDialog() ?? false)
            {
                var viewModel = configurationWindow.ViewModel;
                await Drawing.UpdateSettings(viewModel.IsPublic, viewModel.IsProtected, viewModel.Password);
                ToastsService.Pop("Fantastico!", "Your settings were successfully updated.", Constants.Fireworks);
            }
        }

        private static class Constants
        {
            public const int DefaultCanvasWidth = 550;
            public const int DefaultCanvasHeight = 300;
            public readonly static string Fireworks = "pack://application:,,,/Resources/gears.png";
            public readonly static string PadlockUri = "pack://application:,,,/Resources/padlock.png";
        }
    }
}