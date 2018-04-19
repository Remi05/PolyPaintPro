using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyPaint.Extensions;
using PolyPaint.Services;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Social;
using PolyPaint.Services.Toasts;
using PolyPaint.Utils;
using PolyPaint.Views.Drawing;

namespace PolyPaint.ViewModels.Drawing
{
    public interface IDrawingSelectionViewModel : IRefreshableViewModel
    {
        event Action<Services.Drawing.Drawing> OnDrawingSelected;

        IDrawingsGalleryViewModel DrawingsGalleryViewModel { get; }
        bool IsLoggedIn { get; }

        RelayCommand<object> CreateDrawingCommand { get; }
    }

    public class DrawingSelectionViewModel : ViewModel, IDrawingSelectionViewModel
    {
        public event Action<Services.Drawing.Drawing> OnDrawingSelected;

        private IAuthenticationService AuthService { get; }
        private IToastsService ToastsService { get; }
        private IDrawingService DrawingService { get; }
        private IProfileService ProfileService { get; }
        private IViewsManager ViewsManager { get; }
        private ILogger Logger { get; }

        private IDrawingsGalleryViewModel drawingsGalleryViewModel;
        public IDrawingsGalleryViewModel DrawingsGalleryViewModel
        {
            get => drawingsGalleryViewModel;
            private set { drawingsGalleryViewModel = value; RaisePropertyChanged(); }
        }

        private IDrawingsGalleryViewModel recentDrawingsViewModel;
        public IDrawingsGalleryViewModel RecentDrawingsViewModel
        {
            get => recentDrawingsViewModel;
            private set { recentDrawingsViewModel = value; RaisePropertyChanged(); }
        }

        public bool IsLoggedIn => AuthService.IsLoggedIn;

        public RelayCommand<object> CreateDrawingCommand { get; }

        public DrawingSelectionViewModel(IAuthenticationService authService,
                                         IDrawingService drawingService,
                                         IProfileService profileService,
                                         IToastsService toastsService,
                                         IViewsManager viewsManager,
                                         ILogger logger,
                                         IDrawingsGalleryViewModel drawingsGalleryViewModel,
                                         IDrawingsGalleryViewModel recentDrawingsViewModel)
        {
            AuthService = authService;
            AuthService.CurrentUserChanged += (_) => OnCurrentUserChanged();
            DrawingService = drawingService;
            ProfileService = profileService;
            ToastsService = toastsService;
            ViewsManager = viewsManager;
            Logger = logger;

            DrawingsGalleryViewModel = drawingsGalleryViewModel;
            DrawingsGalleryViewModel.OnDrawingSelected += SelectDrawing;

            RecentDrawingsViewModel = recentDrawingsViewModel;
            RecentDrawingsViewModel.OnDrawingSelected += SelectDrawing;

            CreateDrawingCommand = new RelayCommand<object>((_) => CreateDrawing());
        }

        public async Task Refresh()
        {
            if (AuthService.IsLoggedIn)
            {
                var drawingsIds = new ObservableCollection<string>();
                drawingsIds.AddAll(await DrawingService.GetDrawingsIds(AuthService.CurrentUser.Id, includePrivates: true));
                drawingsIds.AddAll(await DrawingService.GetPublicDrawingsIds(includeProtected: true), true);

                var userDrawingsIds = await ProfileService.GetDrawingsIds(AuthService.CurrentUser.Id, includePrivate: true);

                await RecentDrawingsViewModel.SetDrawingsIds(userDrawingsIds);
                await DrawingsGalleryViewModel.SetDrawingsIds(drawingsIds);

                await Task.Run(async () =>
                 {
                     Thread.Sleep(1500);
                     await RecentDrawingsViewModel.SetDrawingsIds(userDrawingsIds);
                     await DrawingsGalleryViewModel.SetDrawingsIds(drawingsIds);
                 });
            }
            else
            {
                var drawingsIds = new ObservableCollection<string>(await DrawingService.GetPublicDrawingsIds(includeProtected: false));
                await DrawingsGalleryViewModel.SetDrawingsIds(drawingsIds);
            }
        }

        private async void OnCurrentUserChanged()
        {
            RaisePropertyChanged(nameof(IsLoggedIn));
            await Refresh();
        }

        private async void CreateDrawing()
        {
            if (!AuthService.IsLoggedIn)
            {
                var drawing = await DrawingService.CreateDrawing(isPublic: true, isProtected: false, password: null);
                OnDrawingSelected?.Invoke(drawing);
            }
            else
            {
                var wizard = ViewsManager.Get<DrawingConfigurationWizard>();
                var wizardViewModel = wizard.ViewModel;
                if (wizard.ShowDialog() ?? false)
                {
                    var drawing = await DrawingService.CreateDrawing(wizard.ViewModel.IsPublic, wizard.ViewModel.IsProtected, wizard.ViewModel.Password);
                    OnDrawingSelected?.Invoke(drawing);
                }
            }
        }

        private async void SelectDrawing(IDrawingViewModel drawingViewModel)
        {
            if (drawingViewModel == null)
            {
                Logger.Debug("Tried selecting unexisting drawing.");
                return;
            }

            var drawing = await DrawingService.GetDrawing(drawingViewModel.DrawingId);
            if (!drawing.DrawingModel.IsPublic && (AuthService.CurrentUser?.Id == null || drawing.DrawingModel.Owner != AuthService.CurrentUser?.Id))
            {
                drawingViewModel.Hide();
                await Refresh();
                ToastsService.Pop("Sorry!", "This drawing is now private :(", Constants.PadlockUri);
                return;
            }

            if (drawing.DrawingModel.IsProtected && AuthService?.CurrentUser?.Id != drawing.DrawingModel.Owner)
            {
                ProtectedDrawingPasswordPrompt passwordPrompt = ViewsManager.Get<ProtectedDrawingPasswordPrompt>();
                passwordPrompt.ViewModel.PasswordEntered += (password) =>
                {
                    if (password.ToUnsecureString() == drawing.DrawingModel.Password)
                    {
                        passwordPrompt.Close();
                        OnDrawingSelected?.Invoke(drawing);
                    }
                    else
                    {
                        ToastsService.Pop("Nopes", "You entered the wrong password", Constants.PadlockUri);
                    }
                };

                passwordPrompt.ShowDialog();
            }
            else
            {
                OnDrawingSelected?.Invoke(drawing);
            }
        }

        private static class Constants
        {
            public readonly static string PadlockUri = "pack://application:,,,/Resources/padlock.png";
        }
    }
}
