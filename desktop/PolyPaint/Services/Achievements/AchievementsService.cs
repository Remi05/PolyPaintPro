using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyPaint.Services.Achievements
{
    class AchievementsService : IAchievementsService
    {
        private IAuthenticationService AuthService { get; }
        private IDatabaseService DatabaseService { get; }
        private ILogger Logger { get; }

        private ISubscription AchievementsSubscription { get; set; }

        public AchievementsService(IAuthenticationService authService, 
                                   IDatabaseService databaseService,
                                   ILogger logger)
        {
            AuthService = authService;
            DatabaseService = databaseService;
            Logger = logger;
        }

        ~AchievementsService()
        {
            AchievementsSubscription?.Stop();
        }

        public async Task<ObservableDictionary<string, AchievementModel>> GetAchievements()
        {
            var achievementsObservable = new ObservableDictionary<string, AchievementModel>();

            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried getting achievements while logged out.");
                return achievementsObservable;
            }

            Logger.Info($"Getting achievements for user ${AuthService.CurrentUser.DisplayName}");

            var achievementDefinitions = await DatabaseService.Ref(DatabasePaths.Achievements)
                                                              .Child(DatabasePaths.Definitions)
                                                              .Once<Dictionary<string, AchievementModel>>();


            if (achievementDefinitions != null)
            {
                foreach (var achievement in achievementDefinitions)
                {
                    achievementsObservable.Add(achievement);
                }
            }

            var completed = await DatabaseService.Ref(DatabasePaths.Achievements)
                                                 .Child(AuthService.CurrentUser.Id)
                                                 .Child(DatabasePaths.Completed)
                                                 .Once<Dictionary<string, bool>>();

            if (completed != null)
            {
                foreach (var isCompleted in completed)
                {
                    var achievement = achievementsObservable[isCompleted.Key].Completed = isCompleted.Value;
                }
            }

            AchievementsSubscription = DatabaseService.Ref(DatabasePaths.Achievements)
                                                      .Child(AuthService.CurrentUser.Id)
                                                      .Child(DatabasePaths.Completed)
                                                      .OnValue<Dictionary<string, bool>>(achievements =>
                                                      {
                                                          foreach (var isCompleted in achievements)
                                                          {
                                                              var achievement = achievementsObservable[isCompleted.Key];
                                                              if (achievement.Completed)
                                                                  continue;

                                                              achievement.Completed = isCompleted.Value;
                                                              achievementsObservable[isCompleted.Key] = achievement;
                                                          }
                                                      });

            return achievementsObservable;
        }

        public async Task Increment(string userId, string metric)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error($"Tried incrementing achievement metric ${metric} while logged out.");
                return;
            }

            Logger.Info($"Incrementing achievement metric ${metric} for user ${AuthService.CurrentUser.DisplayName}");

            var metricValue = await DatabaseService.Ref(DatabasePaths.Achievements)
                                                   .Child(userId)
                                                   .Child(DatabasePaths.Metrics)
                                                   .Child(metric)
                                                   .Once<int>();

            await DatabaseService.Ref(DatabasePaths.Achievements)
                                 .Child(userId)
                                 .Child(DatabasePaths.Metrics)
                                 .Child(metric)
                                 .Set(metricValue + 1);
        }
    }

    public static class AchievementMetrics
    {
        public readonly static string SharesOnDrive = "sharesOnDrive";
        public readonly static string SharesOnFacebook = "sharesOnFacebook";
        public readonly static string NumberOfNsfwDrawings = "numberOfNsfwDrawings";
    }
}
