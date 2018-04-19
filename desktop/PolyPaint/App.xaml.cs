
using System.Windows;
using PolyPaint.Models;
using PolyPaint.Services;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Messaging;
using PolyPaint.Services.Social;
using PolyPaint.Services.Storage;
using PolyPaint.ViewModels;
using PolyPaint.ViewModels.Auth;
using PolyPaint.ViewModels.Drawing;
using PolyPaint.ViewModels.Social;
using PolyPaint.ViewModels.Messaging;
using PolyPaint.Views;
using Unity;
using PolyPaint.ViewModels.Achievements;
using PolyPaint.Services.Toasts;
using PolyPaint.Services.Achievements;
using PolyPaint.Services.Stories;
using PolyPaint.Services.Email;
using PolyPaint.Services.Cache;
using Unity.ServiceLocation;
using CommonServiceLocator;

namespace PolyPaint
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            IUnityContainer container = new UnityContainer();

            // Services
            container.RegisterSingleton<IAuthenticationService, FirebaseAuthenticationService>();
            container.RegisterSingleton<IDatabaseService, FirebaseDatabaseService>();
            container.RegisterType<ILogger, Logger>();
            container.RegisterType<IExportService, ExportService>();
            container.RegisterType<IToastsService, ToastsService>();
            container.RegisterSingleton<IMessagingService, MessagingService>();
            container.RegisterType<IAchievementsService, AchievementsService>();
            container.RegisterSingleton<IProfileService, ProfileService>();
            container.RegisterSingleton<IStorageService, FirebaseStorageService>();
            container.RegisterType<IViewsManager, ViewsManager>();
            container.RegisterSingleton<IMailService, MailService>();
            container.RegisterType<LocalDrawingService, LocalDrawingService>();
            container.RegisterType<DrawingService, DrawingService>();
            container.RegisterType<IDrawingService, ProxyDrawingService>();
            container.RegisterSingleton<ICacheService, CacheService>();

            // ViewModels
            container.RegisterType<IAppViewModel, AppViewModel>();
            container.RegisterType<IAchievementsViewModel, AchievementsViewModel>();
            container.RegisterType<IChatBoxViewModel, ChatBoxViewModel>();
            container.RegisterType<IChatMessageViewModel, ChatMessageViewModel>();
            container.RegisterType<IConversationPreviewViewModel, ConversationPreviewViewModel>();
            container.RegisterType<IConversationsViewModel, ConversationsViewModel>();
            container.RegisterType<IConversationsListViewModel, ConversationsListViewModel>();
            container.RegisterType<IDrawingEditingViewModel, DrawingEditingViewModel>();
            container.RegisterType<IDrawingConfigurationWizardViewModel, DrawingConfigurationWizardViewModel>();
            container.RegisterType<IDrawingSelectionViewModel, DrawingSelectionViewModel>();
            container.RegisterType<IDrawingViewModel, DrawingViewModel>();
            container.RegisterType<IDrawingsGalleryViewModel, DrawingsGalleryViewModel>();
            container.RegisterType<IHomeViewModel, HomeViewModel>();
            container.RegisterType<ILoginFormsViewModel, LoginFormsViewModel>();
            container.RegisterType<ILoginViewModel, LoginViewModel>();
            container.RegisterType<INewsfeedViewModel, NewsfeedViewModel>();
            container.RegisterType<IPostViewModel, PostViewModel>();
            container.RegisterType<IProfilePageViewModel, ProfilePageViewModel>();
            container.RegisterType<IProfileViewModel, ProfileViewModel>();
            container.RegisterType<IProtectedDrawingPasswordPromptViewModel, ProtectedDrawingPasswordPromptViewModel>();
            container.RegisterType<IRegisterViewModel, RegisterViewModel>();
            container.RegisterType<ISearchPageViewModel, SearchPageViewModel>();
            container.RegisterType<ISearchUserViewModel, SearchUserViewModel>();
            container.RegisterType<ISearchDrawingViewModel, SearchDrawingViewModel>();
            container.RegisterType<IStoriesBarViewModel, StoriesBarViewModel>();
            container.RegisterType<IStoriesService, StoriesService>();
            container.RegisterType<IStoryViewModel, StoryViewModel>();
            container.RegisterType<ITutorialViewModel, TutorialViewModel>();
            container.RegisterType<IUserPreviewViewModel, UserPreviewViewModel>();

            // Setup service locator
            UnityServiceLocator locator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => locator);

            // Other
            container.RegisterType<IEditor, Editor>();

            var appView = container.Resolve<AppView>();
            appView.Show();
        }
    }
}