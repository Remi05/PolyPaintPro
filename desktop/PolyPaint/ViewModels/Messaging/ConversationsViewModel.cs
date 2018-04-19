using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PolyPaint.Services;
using PolyPaint.Services.Messaging;
using PolyPaint.Views.Messaging;

namespace PolyPaint.ViewModels.Messaging
{
    public interface IConversationsViewModel : IViewModel { }

    public class ConversationsViewModel : ViewModel, IConversationsViewModel
    {
        private IMessagingService MessagingService { get; }
        private IViewsManager ViewsManager { get; }

        private ConversationsListView conversationsListView;
        public ConversationsListView ConversationsListView
        {
            get => conversationsListView;
            set { conversationsListView = value; RaisePropertyChanged(); }
        }

        private ObservableCollection<ChatBox> activeChatBoxes = new ObservableCollection<ChatBox>();
        public ObservableCollection<ChatBox> ActiveChatBoxes
        {
            get => activeChatBoxes;
            set { activeChatBoxes = value; RaisePropertyChanged(); }
        }

        public ConversationsViewModel(IMessagingService messagingService, IViewsManager viewsManager)
        {
            MessagingService = messagingService;
            ViewsManager = viewsManager;
            CreateConversationsListView();
            AddPublicChatToActiveChatBoxes();
        }

        private void CreateConversationsListView()
        {
            ConversationsListView = ViewsManager.GetUserControl<ConversationsListView>();
            ConversationsListView.ViewModel.OnConversationSelected += (conversation) => OpenConversation(conversation);
        }

        private async void AddPublicChatToActiveChatBoxes()
        {
            var publicConversation = await MessagingService.GetPublicChannel();
            OpenConversation(publicConversation, false);
        }

        private void OpenConversation(IConversation conversation, bool isMaximized = true)
        {
            if (ActiveChatBoxes.Any(chatbox => chatbox.ViewModel.Conversation.Id == conversation.Id))
                return;

            var chatBoxViewModel = ViewsManager.GetViewModel<IChatBoxViewModel>();
            chatBoxViewModel.SetConversation(conversation);

            var chatBoxWindow = ViewsManager.Get<ChatBoxWindow>();
            var chatBox = ViewsManager.GetUserControl<ChatBox>();
            chatBoxWindow.Owner = App.Current.MainWindow;
            chatBox.ViewModel = chatBoxViewModel;

            chatBoxViewModel.OnClosed += () =>
            {
                ActiveChatBoxes.Remove(chatBox);
            };

            chatBoxViewModel.OnWindowModeToggled += (isWindowMode) =>
            {
                if (isWindowMode)
                {
                    ActiveChatBoxes.Remove(chatBox);
                    chatBox.ViewModel = null;
                    chatBoxWindow.ViewModel = chatBoxViewModel;
                    chatBoxWindow.Show();
                }
                else
                {
                    chatBoxWindow.Hide();
                    chatBoxWindow.ViewModel = null;
                    chatBox.ViewModel = chatBoxViewModel;
                    ActiveChatBoxes.Add(chatBox);
                }
            };

            ActiveChatBoxes.Add(chatBox);

            chatBoxViewModel.IsMaximized = isMaximized;
        }
    }
}
