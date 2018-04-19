using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PolyPaint.Models;

namespace PolyPaint.Services.Social
{
    public interface IProfileService
    {
        Task<string> GetDisplayName(string userId);

        Task<ObservableCollection<string>> GetDrawingsIds(string userId, bool includePrivate);

        Task<ObservableCollection<string>> GetFollowersIds(string userId);

        Task<ObservableCollection<string>> GetFollowingUsersIds(string userId);

        Task<ProfileModel> GetProfile(string userId);

        Task<Dictionary<string, string>> GetUsersStartingWith(string prefix);

        Task SetIsFollowingUser(string userId, bool isFollowing);

        Task<bool?> HasDoneTutorial(string userId);

        Task DoTutorial(string userId);

        Task<ObservableCollection<string>> GetDrawingsIdsNoFilter(string userId);
    }
}
