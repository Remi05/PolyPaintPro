using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Social;
using PolyPaint.Utils;

namespace PolyPaint.ViewModels.Social
{
    public interface IProfileViewModel : IRefreshableViewModel
    {
        string UserId { get; }
        string DisplayName { get; }
        string PhotoUrl { get; }
        ObservableCollection<string> DrawingsIds { get; }
        int NumberOfDrawings { get; }
        int NumberOfFollowers { get; }
        int NumberOfFollowingUsers { get; }
        bool IsCurrentUser { get; }
        bool IsFollowedByCurrentUser { get; }
        bool IsLoading { get; }
        RelayCommand<object> ToggleIsFollowingCommand { get; }

        Task SetUser(string userId);
        Task ToggleIsFollowing();
    }

    public class ProfileViewModel : ViewModel, IProfileViewModel
    {
        private IAuthenticationService AuthService { get; }
        private IProfileService ProfileService { get; }
        private IDrawingService DrawingService { get; }

        private string userId;
        public string UserId
        {
            get => userId;
            private set { userId = value; RaisePropertyChanged(); }
        }

        private ProfileModel model;
        private ProfileModel Model
        {
            get => model;
            set
            {
                model = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(DisplayName));
                RaisePropertyChanged(nameof(PhotoUrl));
            }
        }

        public string DisplayName => Model?.DisplayName;
        public string PhotoUrl => Model?.PhotoUrl;

        private ObservableCollection<string> drawingsIds;
        public ObservableCollection<string> DrawingsIds
        {
            get => drawingsIds;
            set
            {
                drawingsIds = value;
                if (drawingsIds != null)
                {
                    drawingsIds.CollectionChanged += (_, __) => RaisePropertyChanged(nameof(NumberOfDrawings));
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NumberOfDrawings));
            }
        }

        private ObservableCollection<string> followersIds;
        private ObservableCollection<string> FollowersIds
        {
            get => followersIds;
            set
            {
                followersIds = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NumberOfFollowers));
                RaisePropertyChanged(nameof(IsFollowedByCurrentUser));
            }
        }

        private ObservableCollection<string> followingUsersIds;
        private ObservableCollection<string> FollowingUsersIds
        {
            get => followingUsersIds;
            set
            {
                followingUsersIds = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(NumberOfFollowingUsers));
            }
        }

        public int NumberOfDrawings => DrawingsIds?.Count ?? 0;
        public int NumberOfFollowers => FollowersIds?.Count ?? 0;
        public int NumberOfFollowingUsers => FollowingUsersIds?.Count ?? 0;
        public bool IsCurrentUser => UserId != null && AuthService.CurrentUser != null 
                                  && UserId == AuthService.CurrentUser.Id;
        public bool IsFollowedByCurrentUser => AuthService.CurrentUser != null
                                            && (FollowersIds?.Contains(AuthService.CurrentUser.Id) ?? false);

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            private set { isLoading = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> ToggleIsFollowingCommand { get; }

        public ProfileViewModel(IAuthenticationService authService, IProfileService profileService, IDrawingService drawingService)
        {
            AuthService = authService;
            ProfileService = profileService;
            DrawingService = drawingService;

            DrawingsIds = new ObservableCollection<string>();
            FollowersIds = new ObservableCollection<string>();
            FollowingUsersIds = new ObservableCollection<string>();

            ToggleIsFollowingCommand = new RelayCommand<object>(async (_) => await ToggleIsFollowing());
        }

        public async Task Refresh()
        {
            // We only want to show loading spinners 
            // when no data has been loaded yet.
            IsLoading = Model == null;
            if (UserId == null || AuthService.CurrentUser == null)
            {
                Model = null;
                DrawingsIds.Clear();
                FollowersIds.Clear();
                FollowingUsersIds.Clear();
            }
            else
            {
                Model = await ProfileService.GetProfile(UserId);
                DrawingsIds.Update(await DrawingService.GetDrawingsIds(UserId, includePrivates: userId == AuthService.CurrentUser.Id));
                FollowersIds = await ProfileService.GetFollowersIds(UserId);
                FollowingUsersIds = await ProfileService.GetFollowingUsersIds(UserId);
            }
            IsLoading = false;
        }

        public async Task SetUser(string userId)
        {
            UserId = userId;
            await Refresh();
        }

        public async Task ToggleIsFollowing()
        {
            bool isFollowed = IsFollowedByCurrentUser;
            MockToggleIsFollowing();
            await ProfileService.SetIsFollowingUser(UserId, !isFollowed);
            FollowersIds = await ProfileService.GetFollowersIds(UserId);
            FollowingUsersIds = await ProfileService.GetFollowingUsersIds(UserId);
        }     
        
        private void MockToggleIsFollowing()
        {
            if (IsFollowedByCurrentUser)
            {
                FollowersIds.Remove(AuthService.CurrentUser.Id);
            }
            else
            {
                FollowersIds.Add(AuthService.CurrentUser.Id);
            }
            RaisePropertyChanged(nameof(IsFollowedByCurrentUser));
            RaisePropertyChanged(nameof(NumberOfFollowers));
        }
    }
}
