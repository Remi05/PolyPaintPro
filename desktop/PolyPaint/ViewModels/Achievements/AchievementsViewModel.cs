using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Achievements;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Toasts;
using PolyPaint.Utils;

namespace PolyPaint.ViewModels.Achievements
{
    public interface IAchievementsViewModel : IRefreshableViewModel
    {
        List<AchievementModel> Achievements { get; }
        string LockedAchievementIconUri { get; }
    }

    public class AchievementsViewModel : ViewModel, IAchievementsViewModel
    {
        private IAchievementsService AchievementService { get; }
        private IAuthenticationService AuthService { get; }
        private IToastsService ToastsService { get; }

        private ObservableDictionary<string, AchievementModel> achievementsObservable = new ObservableDictionary<string, AchievementModel>();
        private ObservableDictionary<string, AchievementModel> AchievementsObservable
        {
            get => achievementsObservable;
            set
            {
                achievementsObservable = value;
                achievementsObservable.CollectionChanged +=
                    (_, args) =>
                    {
                        if (args.Action == NotifyCollectionChangedAction.Replace || args.Action == NotifyCollectionChangedAction.Add)
                        {
                            var achievement = args.NewItems[0] as AchievementModel;
                            if (achievement.Completed)
                            {
                                ToastsService.Pop(achievement.Name, achievement.Message, achievement.IconUri);
                            }
                        }
                        RaisePropertyChanged(nameof(Achievements));
                    };
                RaisePropertyChanged(nameof(Achievements));
            }
        }

        public List<AchievementModel> Achievements => AchievementsObservable.Values.ToList();

        public string LockedAchievementIconUri => Constants.LockedImageUri;

        public AchievementsViewModel(IAchievementsService achievementsService, IAuthenticationService authService, IToastsService toastsService)
        {
            AchievementService = achievementsService;
            AuthService = authService;
            ToastsService = toastsService;
            RaisePropertyChanged(nameof(LockedAchievementIconUri));
        }

        public async Task Refresh()
        {
            AchievementsObservable = await AchievementService.GetAchievements(); 
        }

        internal static class Constants
        {
            public static readonly string LockedImageUri = "locked.png";
        }
    }
}