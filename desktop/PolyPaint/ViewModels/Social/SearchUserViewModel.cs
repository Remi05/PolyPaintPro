using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PolyPaint.Services;
using PolyPaint.Services.Social;
using PolyPaint.Views.Messaging;

namespace PolyPaint.ViewModels.Social
{
    public interface ISearchUserViewModel : IViewModel
    {
        event Action<string> OnUserClicked;

        ObservableCollection<UserPreview> UsersSearchResults { get; }
        string UserSearchQuery { get; set; }
        string HintMessage { get; }
    }
    class SearchUserViewModel: ViewModel, ISearchUserViewModel
    {
        public event Action<string> OnUserClicked;

        private IProfileService ProfileService { get; }
        private IViewsManager ViewsManager { get; }

        private ObservableCollection<UserPreview> usersSearchResults = new ObservableCollection<UserPreview>();
        public ObservableCollection<UserPreview> UsersSearchResults
        {
            get => usersSearchResults;
            set { usersSearchResults = value; RaisePropertyChanged(); }
        }

        private string userSearchQuery = "";
        public string UserSearchQuery
        {
            get => userSearchQuery;
            set
            {
                userSearchQuery = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HintMessage));
                UpdateUsersSearchResults();
            }
        }

        public string HintMessage => UserSearchQuery.Length == 0 ? "Search a user" : "";

        public SearchUserViewModel(IProfileService profileService, IViewsManager viewsManager)
        {
            ProfileService = profileService;
            ViewsManager = viewsManager;
            UpdateUsersSearchResults();
        }

        private async void UpdateUsersSearchResults()
        {
            string searchQuery = UserSearchQuery;
            var usersToAdd = await ProfileService.GetUsersStartingWith(UserSearchQuery);

            if (searchQuery != UserSearchQuery)
                return;

            UsersSearchResults.Clear();

            if (usersToAdd == null)
                return;

            foreach (KeyValuePair<string, string> user in usersToAdd)
            {
                AddUserResultPreview(user.Key, user.Value);
            }
        }

        private async void AddUserResultPreview(string userId, string displayName)
        {
            var profile = await ProfileService.GetProfile(userId);
            if (profile == null)
                return;

            var userPreview = ViewsManager.GetUserControl<UserPreview>();
            userPreview.ViewModel.Profile = profile;
            userPreview.ViewModel.OnClick += () => OnUserClicked?.Invoke(userId);
            UsersSearchResults.Add(userPreview);
        }
    }
}
