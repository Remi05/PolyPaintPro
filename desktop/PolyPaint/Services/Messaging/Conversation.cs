using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;
using PolyPaint.Utils;

namespace PolyPaint.Services.Messaging
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Conversation : IConversation
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IAuthenticationService AuthService { get; }
        private IDatabaseService DatabaseService { get; }

        private ISubscription MessagesSubscription { get; set; }
        private ObservableCollection<Message> Messages { get; set; }

        private ConversationModel Info { get; set; }
        public string Id => Info.Id;
        public string Name => Info.Name;

        public Conversation(ConversationModel info, IAuthenticationService authService, IDatabaseService databaseService)
        {
            Info = info;
            AuthService = authService;
            DatabaseService = databaseService;
        }

        ~Conversation()
        {
            MessagesSubscription?.Stop();
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddMessageWithAdditionalInfo(Message message)
        {
            if (Messages.Contains(message))
                return;

            message.WasSentByCurrentUser = AuthService.CurrentUser != null 
                                         && message.SenderId == AuthService.CurrentUser.Id;
            Messages.Add(message);
        }

        public async Task<ObservableCollection<Message>> GetMessages()
        {
            var messages = await DatabaseService.Ref(DatabasePaths.Messages)
                                                .Child(Id)
                                                .Once<Dictionary<string, Message>>();

            Messages = new ObservableCollection<Message>();

            if (messages != null)
            {
                foreach (var message in messages.Values)
                {
                    AddMessageWithAdditionalInfo(message);
                }
            }

            MessagesSubscription = DatabaseService.Ref(DatabasePaths.Messages)
                                                  .Child(Id)
                                                  .OnChildAdded((Message message) => AddMessageWithAdditionalInfo(message));

            return Messages;
        }

        public async Task SendMessage(string text)
        {
            if (AuthService.CurrentUser == null)
                return;

            var messageModel = new Message(text, DateTime.Now, AuthService.CurrentUser.Id, AuthService.CurrentUser.DisplayName);
            var addMessageQuery = await DatabaseService.Ref(DatabasePaths.Messages)
                                                       .Child(Id)
                                                       .Push();
            await addMessageQuery.Set(messageModel);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Conversation))
                return false;

            var otherConversation = obj as Conversation;

            return otherConversation == this
                || otherConversation.Info.Equals(Info);
        }
    }
}
