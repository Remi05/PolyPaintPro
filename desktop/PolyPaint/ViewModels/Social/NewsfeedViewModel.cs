using PolyPaint.Services;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Social;
using PolyPaint.Utils;
using PolyPaint.ViewModels.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyPaint.ViewModels.Social
{
    public interface INewsfeedViewModel : IRefreshableViewModel
    {
        IEnumerable<IPostViewModel> SortedPostsViewModels { get; }
        bool IsLoading { get; }
    }

    public class NewsfeedViewModel : ViewModel, INewsfeedViewModel
    {
        private IAuthenticationService AuthService { get; }
        private IDrawingService DrawingService { get; }
        private IProfileService ProfileService { get; }
        private IViewsManager ViewsManager { get; }

        private ICollection<IPostViewModel> PostsViewModels { get; set; } = new List<IPostViewModel>();

        private IEnumerable<IPostViewModel> sortedPostsViewModels;
        public IEnumerable<IPostViewModel> SortedPostsViewModels
        {
            get => sortedPostsViewModels;
            private set { sortedPostsViewModels = value; RaisePropertyChanged(); }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            private set { isLoading = value; RaisePropertyChanged(); }
        }

        public NewsfeedViewModel(IAuthenticationService authService, IDrawingService drawingService, 
                                 IProfileService profileService, IViewsManager viewsManager)
        {
            AuthService = authService;
            DrawingService = drawingService;
            ProfileService = profileService;
            ViewsManager = viewsManager;
        }

        public async Task Refresh()
        {
            IsLoading = true;

            var followingUsersIds = await ProfileService.GetFollowingUsersIds(AuthService.CurrentUser.Id);
            var followingPosts = await CreatePostsForEachUser(followingUsersIds);
            PostsViewModels.Update(followingPosts);
            App.Current.Dispatcher.Invoke(SortPosts);

            var publicDrawingsIds = await DrawingService.GetPublicDrawingsIds(includeProtected: true);
            var publicPosts = await CreatePostsForEachId(publicDrawingsIds);
            PostsViewModels.Update(publicPosts);
            App.Current.Dispatcher.Invoke(SortPosts);

            IsLoading = false;
        }

        private async Task<ICollection<IPostViewModel>> CreatePostsForEachUser(ICollection<string> usersIds)
        {
            var newPostsViewModels = new List<IPostViewModel>();

            if (usersIds == null)
                return newPostsViewModels;

            foreach (string userId in usersIds)
            {
                var profileViewModel = ViewsManager.GetViewModel<IProfileViewModel>();
                await profileViewModel.SetUser(userId);
                newPostsViewModels.AddAll(await CreatePostsViewModels(profileViewModel, profileViewModel.DrawingsIds));
            }

            return newPostsViewModels;
        }

        private async Task<ICollection<IPostViewModel>> CreatePostsViewModels(IProfileViewModel authorProfileViewModel, 
                                                                              ICollection<string> drawingsIds)
        {
            var newPostsViewModels = new List<IPostViewModel>();

            if (drawingsIds == null)
                return newPostsViewModels;

            foreach (string drawingId in drawingsIds)
            {
                var drawingViewModel = ViewsManager.GetViewModel<IDrawingViewModel>();
                await drawingViewModel.SetDrawing(drawingId);

                var postViewModel = ViewsManager.GetViewModel<IPostViewModel>();
                postViewModel.AuthorProfileViewModel = authorProfileViewModel;
                postViewModel.DrawingViewModel = drawingViewModel;

                newPostsViewModels.Add(postViewModel);

                if (newPostsViewModels.Count % Constants.NumberOfPostsBeforeUpdate == 0)
                {
                    PostsViewModels.Update(newPostsViewModels);
                    App.Current.Dispatcher.Invoke(SortPosts);
                }
            }

            return newPostsViewModels;
        }

        public async Task<ICollection<IPostViewModel>> CreatePostsForEachId(ICollection<string> drawingsIds)
        {
            var newPostsViewModels = new List<IPostViewModel>();

            if (drawingsIds == null)
                return newPostsViewModels;

            foreach (string drawingId in drawingsIds)
            {
                var drawingViewModel = ViewsManager.GetViewModel<IDrawingViewModel>();
                await drawingViewModel.SetDrawing(drawingId);

                if (drawingViewModel.Owner == AuthService.CurrentUser.Id)
                    continue;

                var profileViewModel = ViewsManager.GetViewModel<IProfileViewModel>();
                await profileViewModel.SetUser(drawingViewModel.Owner);

                var postViewModel = ViewsManager.GetViewModel<IPostViewModel>();
                postViewModel.AuthorProfileViewModel = profileViewModel;
                postViewModel.DrawingViewModel = drawingViewModel;

                newPostsViewModels.Add(postViewModel);

                if (newPostsViewModels.Count % Constants.NumberOfPostsBeforeUpdate == 0)
                {
                    PostsViewModels.Update(newPostsViewModels);
                    App.Current.Dispatcher.Invoke(SortPosts);
                }
            }

            return newPostsViewModels;
        }

        private void SortPosts()
        {
            SortedPostsViewModels = PostsViewModels.OrderByDescending(post => post, new PostComparer());
        }

        private static class Constants
        {
            public const int NumberOfPostsBeforeUpdate = 3;
        }
    }
}
