using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using PolyPaint.Services;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Social;
using PolyPaint.Utils;
using PolyPaint.Views.Auth;
using PolyPaint.Views.Drawing;
using PolyPaint.Views.Messaging;
using PolyPaint.Views.Social;
using PolyPaint.Views.Achievements;
using PolyPaint.Views.Home;
using PolyPaint.Views;
using PolyPaint.Services.Cache;
using PolyPaint.Services.Email;
using PolyPaint.Services.Drawing;
using System;

namespace PolyPaint.ViewModels
{
    public interface IAppViewModel : IViewModel { }

    public class AppViewModel : ViewModel, IAppViewModel
    {
        public static readonly Dictionary<string, int> PageToIndex = new Dictionary<string, int>
        {
            { nameof(HomeView),                0 },
            { nameof(LoginView),               1 },
            { nameof(ProfilePage),             2 },
            { nameof(Views.Social.SearchPage), 3 },
            { nameof(DrawingEditingView),      4 },
            { nameof(DrawingSelectionView),    4 },
            { nameof(AchievementsView),        5 },
            { nameof(TutorialView),            6 }
        };

        private IAuthenticationService AuthService { get; }
        private IProfileService ProfileService { get; }
        private IDrawingService DrawingService { get; }
        private IViewsManager ViewsManager { get; }
        private ICacheService Cache { get; }
        private IMailService MailService { get; set; }

        private bool IsDrawing { get; set; }

        public bool IsLoggedIn => AuthService.IsLoggedIn;

        public List<bool> isPageActive;
        public List<bool> IsPageActive
        {
            get => isPageActive;
            set { isPageActive = value; RaisePropertyChanged(); }
        }

        private bool isSidebarExpanded;
        public bool IsSidebarExpanded
        {
            get => isSidebarExpanded;
            set { isSidebarExpanded = value; RaisePropertyChanged(); }
        }

        private AchievementsView achievementsPage;
        public AchievementsView AchievementsPage
        {
            get => achievementsPage;
            private set { achievementsPage = value; RaisePropertyChanged(); }
        }

        private ConversationsView conversationsView;
        public ConversationsView ConversationsView
        {
            get => conversationsView;
            private set { conversationsView = value; RaisePropertyChanged(); }
        }

        private DrawingSelectionView drawingSelectionPage;
        public DrawingSelectionView DrawingSelectionPage
        {
            get => drawingSelectionPage;
            private set { drawingSelectionPage = value; RaisePropertyChanged(); }
        }

        private DrawingEditingView drawingPage;
        public DrawingEditingView DrawingPage
        {
            get => drawingPage;
            private set { drawingPage = value; RaisePropertyChanged(); }
        }

        private SearchPage searchPage;
        public SearchPage SearchPage
        {
            get => searchPage;
            set { searchPage = value; RaisePropertyChanged(); }
        }

        private ProfilePage userProfilePage;
        public ProfilePage UserProfilePage
        {
            get => userProfilePage;
            private set { userProfilePage = value; RaisePropertyChanged(); }
        }

        private TutorialView tutorialPage;
        public TutorialView TutorialPage
        {
            get => tutorialPage;
            private set { tutorialPage = value; RaisePropertyChanged(); }
        }

        private HomeView homePage;
        public HomeView HomePage
        {
            get => homePage;
            private set { homePage = value; RaisePropertyChanged(); }
        }

        private LoginView loginPage;
        public LoginView LoginPage
        {
            get => loginPage;
            set { loginPage = value; RaisePropertyChanged(); }
        }

        private UserControl currentPage;
        public UserControl CurrentPage
        {
            get => currentPage;
            private set { currentPage = value; RaisePropertyChanged(); }
        }

        public RelayCommand<UserControl> GoToPageCommand { get; set; }
        public RelayCommand<object> LogoutCommand { get; set; }
        public RelayCommand<object> ToggleSideBarExpandedCommand { get; set; }

        public AppViewModel(IAuthenticationService authService, IProfileService profileService, IViewsManager viewsManager, ICacheService cache, IMailService mailService, IDrawingService drawingService)
        {
            DrawingService = drawingService;
            MailService = mailService;
            AuthService = authService;
            AuthService.CurrentUserChanged += (_) => OnCurrentUserChanged();
            ProfileService = profileService;
            ViewsManager = viewsManager;
            Cache = cache;

            GoToPageCommand = new RelayCommand<UserControl>((page) => GoToPage(page));
            LogoutCommand = new RelayCommand<object>((_) => AuthService.Logout());
            ToggleSideBarExpandedCommand = new RelayCommand<object>((_) => IsSidebarExpanded = !IsSidebarExpanded);

            CreateStaticPages();
            GoToPage(LoginPage);
        }

        private async void OnCurrentUserChanged()
        {
            MailService.EmailCurrentUserOnImageBanned();
            RaisePropertyChanged(nameof(IsLoggedIn));

            if (AuthService.IsLoggedIn)
            {
                CreateLoggedInPages();
                await CheckIfTutorialDone();
            }
            else
            {
                DestroyLoggedInPages();
                if (!IsDrawing)
                {
                    GoToPage(LoginPage);
                }
            }
        }

        private void CreateStaticPages()
        {
            IsPageActive = new List<bool>(new bool[PageToIndex.Count]);

            LoginPage = ViewsManager.GetUserControl<LoginView>();

            DrawingSelectionPage = ViewsManager.GetUserControl<DrawingSelectionView>();
            DrawingSelectionPage.ViewModel.OnDrawingSelected += GoToDrawingPage;

            TutorialPage = ViewsManager.GetUserControl<TutorialView>();
            TutorialPage.ViewModel.GoToDrawingClicked += () => GoToPage(DrawingSelectionPage);
        }

        private void CreateLoggedInPages()
        {
            AchievementsPage = ViewsManager.GetUserControl<AchievementsView>();
            ConversationsView = ViewsManager.GetUserControl<ConversationsView>();
            DrawingSelectionPage.ViewModel.Refresh();
            HomePage = ViewsManager.GetUserControl<HomeView>();
            SearchPage = ViewsManager.GetUserControl<SearchPage>();
            SearchPage.ViewModel.OnUserClicked += GoToUserProfilePage;
            UserProfilePage = ViewsManager.GetUserControl<ProfilePage>();
            UserProfilePage.ViewModel.SetUser(AuthService.CurrentUser.Id);
        }

        private void DestroyLoggedInPages()
        {
            AchievementsPage = null;
            ConversationsView = null;
            HomePage = null;
            SearchPage = null;
            UserProfilePage = null;
        }

        private async Task CheckIfTutorialDone()
        {
            bool didTutorial = await ProfileService.HasDoneTutorial(AuthService.CurrentUser.Id) ?? false;
            if (!didTutorial)
            {
                GoToPage(TutorialPage);
            }
            else
            {
                GoToPage(DrawingSelectionPage);
            }
        }

        private void GoToPage(UserControl userControl, bool saveIfCurrentPageIsDrawing = true)
        {
            if (userControl == null)
                return;

            UpdateCurrentPageArrow(userControl);

            if (saveIfCurrentPageIsDrawing && CurrentPage != null && CurrentPage == DrawingPage)
            {
                (DrawingPage.ViewModel as IDrawingEditingViewModel).SaveDrawingPreview();
            }

            if (userControl == DrawingSelectionPage && IsDrawing)
            {
                userControl = DrawingPage;
            }

            CurrentPage = userControl;
            try
            {
                (CurrentPage.DataContext as IRefreshableViewModel)?.Refresh();
            }
            catch (Exception)
            { }
        }

        private void UpdateCurrentPageArrow(UserControl userControl)
        {
            for (int i = 0; i < IsPageActive.Count; i++)
                IsPageActive[i] = false;

            IsPageActive[PageToIndex[userControl.GetType().Name]] = true;
            RaisePropertyChanged(nameof(IsPageActive));
        }

        private void GoToDrawingPage(Services.Drawing.Drawing drawing)
        {
            DrawingPage = ViewsManager.GetUserControl<DrawingEditingView>();
            DrawingPage.ViewModel.SetDrawing(drawing);
            DrawingPage.ViewModel.GoBackClicked += () => GoToDrawingSelectionPage(true);
            DrawingPage.ViewModel.WasKickedOut += () => GoToDrawingSelectionPage(false);
            if (AuthService?.IsLoggedIn ?? false)
            {
                DrawingService.AddDrawingToUser(AuthService.CurrentUser.Id, drawing.Id);
            }
            IsDrawing = true;
            GoToPage(DrawingPage);
        }

        private void GoToDrawingSelectionPage(bool saveThumbnail)
        {
            IsDrawing = false;
            GoToPage(DrawingSelectionPage, saveIfCurrentPageIsDrawing: saveThumbnail);
        }

        private void GoToUserProfilePage(string userId)
        {
            var profilePage = ViewsManager.GetUserControl<ProfilePage>();
            profilePage.ViewModel.SetUser(userId);
            GoToPage(profilePage);
        }
    }
}
