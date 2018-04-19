using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Social;
using PolyPaint.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace PolyPaint.Services.Stories
{
    class StoriesService : IStoriesService
    {
        private IAuthenticationService AuthService { get; }
        private IDatabaseService DatabaseService { get; }
        private IDrawingService DrawingService { get; }
        private ILogger Logger { get; }
        private IProfileService ProfileService { get; }

        public StoriesService(IAuthenticationService authService, IDatabaseService databaseService,
                              IDrawingService drawingService, ILogger logger, IProfileService profileService)
        {
            AuthService = authService;
            DatabaseService = databaseService;
            DrawingService = drawingService;
            Logger = logger;
            ProfileService = profileService;
        }

        public async Task<StoryModel> GetStory(string userId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting Story of user ${userId} while logged out.");
                return null;
            }

            Logger.Info($"Getting Story of user ${userId} for user ${AuthService.CurrentUser.DisplayName}");

            return await DatabaseService.Ref(DatabasePaths.Stories)
                                        .Child(userId)
                                        .Once<StoryModel>();
        }

        public async Task AppendToStory(string drawingId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried adding drawing ${drawingId} to Story while logged out.");
                return;
            }

            Logger.Info($"Adding drawing ${drawingId} to Story for user ${AuthService.CurrentUser.DisplayName}");

            var story = await GetStory(AuthService.CurrentUser.Id);
            if (!IsStoryValid(story))
            {
                story = new StoryModel
                {
                    ExpirationDate = DateTime.Now.AddSeconds(Constants.StoryDurationInSeconds),
                    Drawings = new Dictionary<string, int>()
                };

            }

            var nextIndex = story.Drawings.Count == 0 ? 0 : story.Drawings.Values.Max() + 1;
            story.Drawings[drawingId] = nextIndex;

            await DatabaseService.Ref(DatabasePaths.Stories)
                                 .Child(AuthService.CurrentUser.Id)
                                 .Set(story);
        }

        public async Task RemoveFromStory(string drawingId)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried removing drawing ${drawingId} to Story while logged out.");
                return;
            }

            Logger.Info($"Removing drawing ${drawingId} to Story for user ${AuthService.CurrentUser.DisplayName}");

            await DatabaseService.Ref(DatabasePaths.Stories)
                                 .Child(AuthService.CurrentUser.Id)
                                 .Child(DatabasePaths.Drawings)
                                 .Set<object>(null);
        }

        public async Task<ObservableCollection<DetailedStoryModel>> GetStories()
        {
            var stories = new ObservableCollection<DetailedStoryModel>();

            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting followers' Stories while logged out.");
                return stories;
            }

            Logger.Info($"Getting followers' Stories for user ${AuthService.CurrentUser.DisplayName}");

            var followings = await ProfileService.GetFollowingUsersIds(AuthService.CurrentUser.Id);

            foreach (var userId in followings)
            {
                var story = await GetStory(userId);
                if (!IsStoryValid(story))
                    continue;

                stories.Add(await ToDetailedStoryModel(userId, story));
            }

            return stories;
        }

        private async Task<DetailedStoryModel> ToDetailedStoryModel(string userId, StoryModel story)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried converting Story of user ${userId} to DetailedStory while logged out.");
                return null;
            }

            Logger.Info($"Converting Story of user ${userId} to DetailedStory for user ${AuthService.CurrentUser.DisplayName}");

            var owner = await ProfileService.GetProfile(userId);

            var items = story?.Drawings?.ToList();
            items.Sort((x, y) => x.Value - y.Value);
            var orderedDrawingIds = new List<string>(items.Select(x => x.Key));

            var drawingPreviewUrls = new List<string>();
            foreach (var drawingId in orderedDrawingIds)
            {
                var drawingInfo = await DrawingService.GetDrawingInfo(drawingId);
                drawingPreviewUrls.Add(drawingInfo?.PreviewUrl);
            }

            return new DetailedStoryModel()
            {
                Owner = owner,
                Drawings = story?.Drawings,
                ExpirationDate = story?.ExpirationDate ?? DateTime.Now,
                DrawingPreviewUrls = drawingPreviewUrls
            };
        }

        private bool IsStoryValid(StoryModel story)
        {
            return story?.Drawings != null && DateTime.Now <= story.ExpirationDate;
        }

        private static class Constants
        {
            public static readonly int StoryDurationInSeconds = 60 * 60 * 24;
        }
    }
}
