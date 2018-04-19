using PolyPaint.Models;
using PolyPaint.Services;
using PolyPaint.Utils;
using PolyPaint.ViewModels.Social;
using System.Threading.Tasks;

namespace PolyPaint.ViewModels.Achievements
{

    public interface IHomeViewModel : IRefreshableViewModel
    {
        INewsfeedViewModel NewsfeedViewModel { get; }
        IStoriesBarViewModel StoriesBarViewModel { get; }
        IStoryViewModel StoryViewModel { get; }
    }

    public class HomeViewModel : ViewModel, IHomeViewModel
    {
        IViewsManager ViewsManager { get; }

        private INewsfeedViewModel newsfeedViewModel;
        public INewsfeedViewModel NewsfeedViewModel
        {
            get => newsfeedViewModel;
            private set { newsfeedViewModel = value; RaisePropertyChanged(); }
        }

        private IStoriesBarViewModel storiesBarViewModel;
        public IStoriesBarViewModel StoriesBarViewModel
        {
            get => storiesBarViewModel;
            private set { storiesBarViewModel = value; RaisePropertyChanged(); }
        }

        private IStoryViewModel storyViewModel;
        public IStoryViewModel StoryViewModel
        {
            get => storyViewModel;
            private set
            {
                storyViewModel = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsShowingStory));
            }
        }

        public bool IsShowingStory => StoryViewModel != null;

        public RelayCommand<DetailedStoryModel> ShowStoryCommand { get; set; }
        public RelayCommand<object> HideStoryCommand { get; set; }

        public HomeViewModel(INewsfeedViewModel newsfeedViewModel, 
                             IStoriesBarViewModel storiesBarViewModel,
                             IViewsManager viewsManager)
        {
            NewsfeedViewModel = newsfeedViewModel;
            StoriesBarViewModel = storiesBarViewModel;
            ViewsManager = viewsManager;

            ShowStoryCommand = new RelayCommand<DetailedStoryModel>(ShowStory);
            HideStoryCommand = new RelayCommand<object>((_) => HideStory());
        }

        public async Task Refresh()
        {
            await StoriesBarViewModel?.Refresh();
            await NewsfeedViewModel?.Refresh();
        }

        private void HideStory()
        {
            StoryViewModel = null;
        }

        private void ShowStory(DetailedStoryModel story)
        {
            StoryViewModel = ViewsManager.GetViewModel<IStoryViewModel>();
            StoryViewModel.Story = story;
            StoryViewModel.OnClose += HideStory;
        }
    }
}