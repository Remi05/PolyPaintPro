using System;
using PolyPaint.Services;
using PolyPaint.Services.Drawing;
using PolyPaint.Utils;
using PolyPaint.ViewModels.Drawing;

namespace PolyPaint.ViewModels.Social
{
    public interface ISearchDrawingViewModel : IViewModel
    {
        IDrawingsGalleryViewModel DrawingsGalleryViewModel { get; }
        IPostViewModel PostViewModel { get; }
        bool IsShowingPost { get; }
        string DrawingSearchQuery { get; set; }
        string HintMessage { get; }

        RelayCommand<object> HidePostCommand { get; }
    }

    public class SearchDrawingViewModel : ViewModel, ISearchDrawingViewModel
    {
        private IDrawingService DrawingService { get; }
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

        private bool isShowingPost;
        public bool IsShowingPost
        {
            get => isShowingPost;
            private set { isShowingPost = value; RaisePropertyChanged(); }
        }

        private string drawingSearchQuery = "";
        public string DrawingSearchQuery
        {
            get => drawingSearchQuery;
            set
            {
                drawingSearchQuery = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HintMessage));
                UpdateDrawingSearchResults();
            }
        }

        public string HintMessage => DrawingSearchQuery.Length == 0 ? "Search a drawing" : "";

        public RelayCommand<object> HidePostCommand { get; }

        public SearchDrawingViewModel(IDrawingService drawingService,
                                      IViewsManager viewsManager,
                                      IDrawingsGalleryViewModel drawingsGalleryViewModel)
        {
            DrawingService = drawingService;
            ViewsManager = viewsManager;

            DrawingsGalleryViewModel = drawingsGalleryViewModel;
            DrawingsGalleryViewModel.OnDrawingSelected += ShowPost;

            HidePostCommand = new RelayCommand<object>((_) => HidePost());

            UpdateDrawingSearchResults();
        }

        private void ShowPost(IDrawingViewModel drawingViewModel)
        {
            PostViewModel = ViewsManager.GetViewModel<IPostViewModel>();
            PostViewModel.AuthorProfileViewModel = ViewsManager.GetViewModel<IProfileViewModel>();
            PostViewModel.AuthorProfileViewModel.SetUser(drawingViewModel.Owner);
            PostViewModel.DrawingViewModel = drawingViewModel;
            IsShowingPost = true;
        }

        private void HidePost()
        {
            IsShowingPost = false;
            PostViewModel = null;
        }

        private async void UpdateDrawingSearchResults()
        {
            string searchQuery = DrawingSearchQuery;
            var drawingIds = await DrawingService.GetDrawingIdTaggedAs(DrawingSearchQuery);

            if (searchQuery != DrawingSearchQuery 
             || drawingIds == null
             || DrawingsGalleryViewModel == null)
                return;

            await DrawingsGalleryViewModel.SetDrawingsIds(drawingIds);
        }
    }
}
