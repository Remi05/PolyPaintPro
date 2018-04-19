using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Social;
using PolyPaint.Services.Stories;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PolyPaint.ViewModels.Social
{
    public interface IStoriesBarViewModel : IRefreshableViewModel
    {
        ObservableCollection<DetailedStoryModel> Stories { get; set; }
    }

    public class StoriesBarViewModel : ViewModel, IStoriesBarViewModel
    {
        IAuthenticationService AuthService { get; }
        IProfileService ProfileService { get; }
        IStoriesService StoriesService { get; }

        public bool HasStoriesOrDoesNotFollowPeople => Stories?.Count > 0 || !FollowsPeople;

        private bool followsPeople;
        public bool FollowsPeople
        {
            get => followsPeople;
            set
            {
                followsPeople = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasStoriesOrDoesNotFollowPeople));
            }
        }

        private ObservableCollection<DetailedStoryModel> stories;
        public ObservableCollection<DetailedStoryModel> Stories
        {
            get => stories;
            set
            {
                stories = value;
                if (stories != null)
                {
                    stories.CollectionChanged += (_, __) => RaisePropertyChanged(nameof(Stories));
                }
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasStoriesOrDoesNotFollowPeople));
            }
        }


        public StoriesBarViewModel(IAuthenticationService authService, 
                                   IProfileService profileService,
                                   IStoriesService storiesService)
        {
            AuthService = authService;
            AuthService.CurrentUserChanged += async (_) => await Refresh();
            ProfileService = profileService;
            StoriesService = storiesService;
        }

        public async Task Refresh()
        {
            if (AuthService.CurrentUser == null)
            {
                Stories = null;
                FollowsPeople = false;
            }
            else
            {
                Stories = await StoriesService.GetStories();
                var followingIds = await ProfileService.GetFollowingUsersIds(AuthService.CurrentUser.Id);
                FollowsPeople = followingIds?.Count > 0;
            }
        }
    }
}
