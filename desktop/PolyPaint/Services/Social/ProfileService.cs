using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Utils;

namespace PolyPaint.Services.Social
{
    public class ProfileService : IProfileService
    {
        private static class Constants
        {
            public static readonly string StartsWithQuerySuffix = @"\uf8ff";
        }

        private IAuthenticationService AuthService { get; }
        private IDatabaseService DatabaseService { get; }
        private ILogger Logger { get; }

        public ProfileService(IAuthenticationService authService, IDatabaseService databaseService, ILogger logger)
        {
            AuthService = authService;
            DatabaseService = databaseService;
            Logger = logger;

            UpdateCurrentUserDisplayName();
            UpdateCurrentUserProfile();

            AuthService.CurrentUserChanged += (user) =>
            {
                UpdateCurrentUserDisplayName();
                UpdateCurrentUserProfile();
            };
        }

        private async Task SetDisplayName(string displayName)
        {
            await DatabaseService.Ref(DatabasePaths.DisplayNames)
                                 .Child(AuthService.CurrentUser.Id)
                                 .Set(displayName);
        }

        private async void UpdateCurrentUserDisplayName()
        {
            if (AuthService.CurrentUser == null)
                return;

            await SetDisplayName(AuthService.CurrentUser.DisplayName);
        }

        private async void UpdateCurrentUserProfile()
        {
            if (AuthService.CurrentUser == null)
                return;

            var profile = new ProfileModel(AuthService.CurrentUser.DisplayName,
                                           AuthService.CurrentUser.PhotoUrl,
                                           AuthService.CurrentUser.Id);

            await DatabaseService.Ref(DatabasePaths.Users)
                                    .Child(AuthService.CurrentUser.Id)
                                    .Child(DatabasePaths.Profile)
                                    .Set(profile);
        }

        public async Task<string> GetDisplayName(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting displayName of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting displayName of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            return await DatabaseService.Ref(DatabasePaths.DisplayNames)
                                        .Child(userId)
                                        .Once<string>();
        }

        public async Task<ObservableCollection<string>> GetDrawingsIds(string userId, bool includePrivate = false)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting drawings IDs of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting drawings IDs of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            var drawingsIdsDict = await DatabaseService.Ref(DatabasePaths.Users)
                                                       .Child(userId)
                                                       .Child(DatabasePaths.Drawings)
                                                       .Once<Dictionary<string, string>>();

            var drawingInfo = await DatabaseService.Ref(DatabasePaths.DrawingInfo).Once<Dictionary<string, DrawingInfo>>();
            var drawings = await DatabaseService.Ref(DatabasePaths.Drawings).Once<Dictionary<string, DrawingModel>>();

            if (drawingsIdsDict == null)
                return new ObservableCollection<string>();

            var ids = drawingsIdsDict.Where(x => (drawingInfo?.ContainsKey(x.Key) ?? false)
                                                  && !drawingInfo[x.Key].IsBanned
                                                  && (drawings[x.Key].IsPublic
                                                      || includePrivate && drawings[x.Key].Owner == AuthService?.CurrentUser?.Id)
                                                  ).Select(x => x.Key).ToList();

            var drawingsIds = new ObservableCollection<string>();
            drawingsIds.AddAll(ids);

            return drawingsIds;
        }


        public async Task<ObservableCollection<string>> GetDrawingsIdsNoFilter(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting drawings IDs of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting drawings IDs of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            var drawingsIdsDict = await DatabaseService.Ref(DatabasePaths.Users)
                                                       .Child(userId)
                                                       .Child(DatabasePaths.Drawings)
                                                       .Once<Dictionary<string, string>>();
            if (drawingsIdsDict == null)
                return new ObservableCollection<string>();
            var drawingsIds = new ObservableCollection<string>();
            drawingsIds.AddAll(drawingsIdsDict?.Keys);

            return drawingsIds;
        }

        public async Task<ObservableCollection<string>> GetFollowersIds(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting followers IDs of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting followers IDs of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            var followersIdsDict = await DatabaseService.Ref(DatabasePaths.Users)
                                                        .Child(userId)
                                                        .Child(DatabasePaths.Followers)
                                                        .Once<Dictionary<string, bool>>();

            var followersIds = new ObservableCollection<string>();
            followersIds.AddAll(followersIdsDict?.Keys);

            return followersIds;
        }

        public async Task<ObservableCollection<string>> GetFollowingUsersIds(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting following users IDs of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting following users IDs of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            var followingUsersIdsDict = await DatabaseService.Ref(DatabasePaths.Users)
                                                      .Child(userId)
                                                      .Child(DatabasePaths.Following)
                                                      .Once<Dictionary<string, bool>>();

            var followingIds = new ObservableCollection<string>();
            followingIds.AddAll(followingUsersIdsDict?.Keys);

            return followingIds;
        }

        public async Task<ProfileModel> GetProfile(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting profile of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting profile of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            var profile = await DatabaseService.Ref(DatabasePaths.Users)
                                        .Child(userId)
                                        .Child(DatabasePaths.Profile)
                                        .Once<ProfileModel>();

            if (profile == null
                && AuthService.CurrentUser != null
                && userId == AuthService.CurrentUser.Id)
            {
                profile = new ProfileModel(AuthService.CurrentUser.DisplayName,
                                      AuthService.CurrentUser.PhotoUrl,
                                      AuthService.CurrentUser.Id);

                await DatabaseService.Ref(DatabasePaths.Users)
                                     .Child(AuthService.CurrentUser.Id)
                                     .Child(DatabasePaths.Profile)
                                     .Set(profile);
            }

            return profile;
        }

        public async Task SetIsFollowingUser(string userId, bool isFollowing)
        {
            if (userId == null)
            {
                Logger.Error($"Tried setting isFollowing to ${isFollowing} for a null user.");
                return;
            }

            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried setting isFollowing user ${userId} to ${isFollowing} while logged out.");
                return;
            }

            Logger.Info($"Setting isFollowing user ${userId} to ${isFollowing} for user ${AuthService.CurrentUser.DisplayName}");

            var followers = new Dictionary<string, bool>() {
                { AuthService.CurrentUser.Id, isFollowing }
            };

            await DatabaseService.Ref(DatabasePaths.Users)
                                 .Child(userId)
                                 .Child(DatabasePaths.Followers)
                                 .Update(followers);

            var following = new Dictionary<string, bool>() {
                { userId, isFollowing }
            };

            await DatabaseService.Ref(DatabasePaths.Users)
                                 .Child(AuthService.CurrentUser.Id)
                                 .Child(DatabasePaths.Following)
                                 .Update(following);
        }

        public async Task<Dictionary<string, string>> GetUsersStartingWith(string prefix)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting users starting with ${prefix} while logged out.");
                return null;
            }

            Logger.Info($"Getting users starting with ${prefix} for user ${AuthService.CurrentUser.DisplayName}");

            var displayNamesDict = await DatabaseService.Ref(DatabasePaths.DisplayNames)
                                                        .Once<Dictionary<string, string>>();

            return displayNamesDict?.Where(kvp => kvp.Value.ToLower()
                                    .Contains(prefix.ToLower()))
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public async Task<bool?> HasDoneTutorial(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting hasDoneTutorial of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting hasDoneTutorial of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            return await DatabaseService.Ref(DatabasePaths.Users)
                                        .Child(userId)
                                        .Child(DatabasePaths.DidTutorial)
                                        .Once<bool?>();
        }

        public async Task DoTutorial(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried setting hasDoneTutorial of user ${userId} while logged out.");
                return;
            }

            Logger.Info($"Setting hasDoneTutorial of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            await DatabaseService.Ref(DatabasePaths.Users)
                                 .Child(userId)
                                 .Child(DatabasePaths.DidTutorial)
                                 .Set<bool?>(true);
        }
    }
}
