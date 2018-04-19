using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Social;
using PolyPaint.Utils;

namespace PolyPaint.Services.Messaging
{
    public class MessagingService : IMessagingService
    {
        private static class Constants
        {
            public static readonly string PublicConversationId = "public";
        }

        private IAuthenticationService AuthService { get; }
        private IDatabaseService DatabaseService { get; }
        private ILogger Logger { get; }
        private IProfileService ProfileService { get; }
        private ISubscription ConversationsSubscription { get; set; }

        public MessagingService(IAuthenticationService authService, IDatabaseService databaseService,
                                ILogger logger, IProfileService profileService)
        {
            AuthService = authService;
            DatabaseService = databaseService;
            Logger = logger;
            ProfileService = profileService;
            AuthService.CurrentUserChanged += (_) =>
            {
                ConversationsSubscription?.Stop();
                ConversationsSubscription = null;
            };
        }

        ~MessagingService()
        {
            ConversationsSubscription?.Stop();
        }

        public async Task<IConversation> CreateConversation(string conversationName)
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error("Tried creating conversation while logged out.");
                return null;
            }

            Logger.Info($"Creating conversation for user ${AuthService.CurrentUser.DisplayName}");

            var addConversationQuery = await DatabaseService.Ref(DatabasePaths.Conversations).Push();
            var conversationId = addConversationQuery.Key;
            var conversationInfo = new ConversationModel(conversationId, conversationName);
            await addConversationQuery.Set(conversationInfo);

            return new Conversation(conversationInfo, AuthService, DatabaseService);
        }

        public async Task<ObservableCollection<IConversation>> GetConversations()
        {
            var conversations = new ObservableCollection<IConversation>();

            if (!AuthService.IsLoggedIn)
            {
                Logger.Error("Tried getting conversations while logged out.");
                return conversations;
            }

            Logger.Info($"Getting conversations for user ${AuthService.CurrentUser.DisplayName}");

            if (AuthService.CurrentUser == null)
                return conversations;

            var conversationModels = await DatabaseService.Ref(DatabasePaths.Conversations)
                                                          .Once<Dictionary<string, ConversationModel>>() ?? new Dictionary<string, ConversationModel>();

            foreach (var conversationModel in conversationModels.Values)
            {
                conversations.Add(new Conversation(conversationModel, AuthService, DatabaseService));
            }

            ConversationsSubscription = DatabaseService.Ref(DatabasePaths.Conversations)
                                                       .OnChildAdded<ConversationModel>(conversationModel =>
                                                       {
                                                           var newConversation = new Conversation(conversationModel, AuthService, DatabaseService);
                                                           if (!conversations.Contains(newConversation))
                                                           {
                                                               conversations.Add(newConversation);
                                                           }
                                                       });

            return conversations;
        }

        public async Task<IConversation> GetPublicChannel()
        {
            if (!AuthService.IsLoggedIn)
            {
                Logger.Error("Tried getting public channel while logged out.");
                return null;
            }

            Logger.Info($"Getting pubic channel for user ${AuthService.CurrentUser.DisplayName}");

            var conversationModel = await DatabaseService.Ref(DatabasePaths.Conversations)
                                                         .Child(Constants.PublicConversationId)
                                                         .Once<ConversationModel>();

            return new Conversation(conversationModel, AuthService, DatabaseService);
        }
    }
}
