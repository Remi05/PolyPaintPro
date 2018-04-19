using GalaSoft.MvvmLight.Command;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Social;
using PolyPaint.Services.Toasts;
using System;

namespace PolyPaint.ViewModels
{
    public interface ITutorialViewModel : IViewModel
    {
        event Action GoToDrawingClicked;

        string CurrentPicture { get; }
        string CurrentPictureSource { get; }
        string TotalPictures { get; }
        bool CanGoToNextPicture { get; }
        bool CanGoToPreviousPicture { get; }
        bool IsTutorialDone { get; }

        RelayCommand<object> GoToDrawingPageCommand { get; }
        RelayCommand<object> NextPictureCommand { get; }
        RelayCommand<object> PreviousPictureCommand { get; }
    }

    class TutorialViewModel : ViewModel, ITutorialViewModel
    {
        public event Action GoToDrawingClicked;

        IAuthenticationService AuthService { get; }
        IProfileService ProfileService { get; }
        IToastsService ToastsService { get; }

        private int currentPictureId;
        private int CurrentPictureId
        {
            get => currentPictureId;
            set
            {
                if (value < 0 || value >= Constants.PictureQuantity)
                    return;

                currentPictureId = value;
                RaisePropertyChanged(nameof(CurrentPicture));
                RaisePropertyChanged(nameof(CurrentPictureSource));
                NextPictureCommand.RaiseCanExecuteChanged();
                PreviousPictureCommand.RaiseCanExecuteChanged();
            }
        }

        public string CurrentPicture => (CurrentPictureId + 1).ToString();
        public string CurrentPictureSource => Constants.PhotoPath + Constants.PhotoNamePrefix + CurrentPictureId.ToString() + Constants.Extension;
        public string TotalPictures => Constants.PictureQuantity.ToString();

        public bool CanGoToNextPicture => CurrentPictureId < Constants.PictureQuantity - 1;
        public bool CanGoToPreviousPicture => CurrentPictureId > 0;

        private bool isTutorialDone;
        public bool IsTutorialDone
        {
            get => isTutorialDone;
            private set { isTutorialDone = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> GoToDrawingPageCommand { get; }
        public RelayCommand<object> NextPictureCommand { get; }
        public RelayCommand<object> PreviousPictureCommand { get; }

        public TutorialViewModel(IAuthenticationService authService, IProfileService profileService, IToastsService toastsService)
        {
            AuthService = authService;
            ProfileService = profileService;
            ToastsService = toastsService;

            GoToDrawingPageCommand = new RelayCommand<object>((_) => GoToDrawing());
            NextPictureCommand = new RelayCommand<object>((_) => NextPicture(), (_) => CanGoToNextPicture);
            PreviousPictureCommand = new RelayCommand<object>((_) => PreviousPicture(), (_) => CanGoToPreviousPicture);

            CurrentPictureId = 0;
        }

        private void GoToDrawing()
        {
            CurrentPictureId = 0;
            IsTutorialDone = false;
            GoToDrawingClicked?.Invoke();
        }

        private async void NextPicture()
        {
            ++CurrentPictureId;

            if (CurrentPictureId >= Constants.PictureQuantity - 1 && !IsTutorialDone)
            {
                IsTutorialDone = true;
                if(AuthService.CurrentUser != null)
                {
                    bool? isDone = await ProfileService.HasDoneTutorial(AuthService.CurrentUser.Id);
                    if (!isDone ?? true)
                    {
                        ToastsService.Pop("You're good to go!", "You've seen the whole tutorial, you're ready to draw!", Constants.ToastPath);
                        await ProfileService.DoTutorial(AuthService.CurrentUser.Id);
                    }
                }
            }
        }

        private void PreviousPicture()
        {
            --CurrentPictureId;
        }

        private class Constants
        {
            public static readonly string PhotoPath = "/Resources/Tutorial/";
            public static readonly string ToastPath = "pack://application:,,,/Resources/fireworks.png";
            public static readonly string PhotoNamePrefix = "image";
            public static readonly string Extension = ".png";
            public static readonly int PictureQuantity = 20;
        }
    }
}
