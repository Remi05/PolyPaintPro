using System.Threading.Tasks;
using PolyPaint.Services;
using PolyPaint.Utils;
using PolyPaint.ViewModels.Drawing;

namespace PolyPaint.ViewModels.Social
{
    public interface IProfilePageViewModel : IRefreshableViewModel
    {
        IDrawingsGalleryViewModel DrawingsGalleryViewModel { get; }
        IPostViewModel PostViewModel { get; }
        IProfileViewModel ProfileViewModel { get; }
        bool IsShowingPost { get; }
        RelayCommand<object> HidePostCommand { get; }

        Task SetUser(string userId);
    }

    public class ProfilePageViewModel : ViewModel, IProfilePageViewModel
    {
        private IViewsManager ViewsManager { get; }

        private IDrawingsGalleryViewModel drawingsGalleryViewModel;
        public IDrawingsGalleryViewModel DrawingsGalleryViewModel
        {
            get => drawingsGalleryViewModel;
            private set { drawingsGalleryViewModel = value; RaisePropertyChanged(); }
        }

        private IPostViewModel postViewModel;
        public IPostViewModel PostViewModel
        {
            get => postViewModel;
            private set { postViewModel = value; RaisePropertyChanged(); }
        }

        private IProfileViewModel profileViewModel;
        public IProfileViewModel ProfileViewModel
        {
            get => profileViewModel;
            private set { profileViewModel = value; RaisePropertyChanged(); }
        }

        private string UserId { get; set; }

        private bool isShowingPost;
        public bool IsShowingPost
        {
            get => isShowingPost;
            private set { isShowingPost = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> HidePostCommand { get; }

        public ProfilePageViewModel(IDrawingsGalleryViewModel drawingsGalleryViewModel, 
                                    IProfileViewModel profileViewModel,
                                    IViewsManager viewsManager)
        {
            DrawingsGalleryViewModel = drawingsGalleryViewModel;
            ProfileViewModel = profileViewModel;
            ViewsManager = viewsManager;

            DrawingsGalleryViewModel.OnDrawingSelected += ShowPost;

            HidePostCommand = new RelayCommand<object>((_) => HidePost());
        }

        public async Task Refresh()
        {
            await ProfileViewModel.SetUser(UserId);
            await DrawingsGalleryViewModel.SetDrawingsIds(ProfileViewModel.DrawingsIds);
        }

        public async Task SetUser(string userId)
        {
            UserId = userId;
            await Refresh();
        }

        private void HidePost()
        {
            IsShowingPost = false;
            PostViewModel = null;
        }

        private void ShowPost(IDrawingViewModel drawingViewModel)
        {
            PostViewModel = ViewsManager.GetViewModel<IPostViewModel>();
            PostViewModel.AuthorProfileViewModel = ProfileViewModel;
            PostViewModel.DrawingViewModel = drawingViewModel;
            IsShowingPost = true;
        }
    }
}
