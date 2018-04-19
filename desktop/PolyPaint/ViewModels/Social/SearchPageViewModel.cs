using System;
using PolyPaint.Utils;

namespace PolyPaint.ViewModels.Social
{
    public interface ISearchPageViewModel : IViewModel
    {
        event Action<string> OnUserClicked;

        ISearchDrawingViewModel SearchDrawingViewModel { get; }
        ISearchUserViewModel SearchUserViewModel { get; }
        bool IsSearchingDrawings { get; }
        bool IsSearchingUsers { get; }
    }
    public class SearchPageViewModel : ViewModel, ISearchPageViewModel
    {
        public event Action<string> OnUserClicked;

        public ISearchDrawingViewModel SearchDrawingViewModel { get; }
        public ISearchUserViewModel SearchUserViewModel { get; }

        private bool isSearchingDrawings;
        public bool IsSearchingDrawings
        {
            get => isSearchingDrawings;
            private set { isSearchingDrawings = value; RaisePropertyChanged(); }
        }

        private bool isSearchingUsers = true;
        public bool IsSearchingUsers
        {
            get => isSearchingUsers;
            private set { isSearchingUsers = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> ToggleSearchViewCommand { get; }

        public SearchPageViewModel(ISearchDrawingViewModel searchDrawingViewModel, 
                                   ISearchUserViewModel searchUserViewModel)
        {
            SearchDrawingViewModel = searchDrawingViewModel;
            SearchUserViewModel = searchUserViewModel;
            SearchUserViewModel.OnUserClicked += (userId) => OnUserClicked?.Invoke(userId);
            ToggleSearchViewCommand = new RelayCommand<object>((_) => ToggleSearchView());
        }

        private void ToggleSearchView()
        {
            IsSearchingDrawings = !IsSearchingDrawings;
            IsSearchingUsers = !IsSearchingUsers;
        }
    }
}