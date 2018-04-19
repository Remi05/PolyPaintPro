using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PolyPaint.Models;
using PolyPaint.Services;
using PolyPaint.Services.Messaging;
using PolyPaint.Services.Social;
using PolyPaint.Utils;
using PolyPaint.Views.Messaging;

namespace PolyPaint.ViewModels.Messaging
{
    public interface IChatBoxViewModel : IViewModel
    {
        event Action OnClosed;
        event Action OnMessageReceived;
        event Action<bool> OnWindowModeToggled;

        IConversation Conversation { get; }
        ObservableCollection<ChatMessage> MessageViews { get; }

        string MessageText { get; set; }
        string WriteMessageHint { get; }
        bool IsLoading { get; set; }
        bool IsMaximized { get; set; }
        bool IsWindowMode { get; }
        bool CanBeClosed { get; }
        bool CanBeCollapsed { get; }
        bool CanSendMessage { get; }
        bool HasMessages { get; }

        RelayCommand<object> CloseCommand { get; }
        RelayCommand<object> SendMessageCommand { get; }
        RelayCommand<object> ToggleMaximizedCommand { get; }
        RelayCommand<object> ToggleWindowModeCommand { get; }

        void SetConversation(IConversation conversation);
    }

    public class ChatBoxViewModel : ViewModel, IChatBoxViewModel
    {
        public event Action OnClosed;
        public event Action OnMessageReceived;
        public event Action<bool> OnWindowModeToggled;

        private IViewsManager ViewsManager { get; }

        private IConversation conversation;
        public IConversation Conversation
        {
            get => conversation;
            private set
            {
                conversation = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanBeClosed));
            }
        }

        private ObservableCollection<ChatMessage> messageViews = new ObservableCollection<ChatMessage>();
        public ObservableCollection<ChatMessage> MessageViews
        {
            get => messageViews;
            private set
            {
                messageViews = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasMessages));
            }
        }

        private string messageText = "";
        public string MessageText
        {
            get => messageText; 
            set
            {
                messageText = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(WriteMessageHint));
                RaisePropertyChanged(nameof(CanSendMessage));
            }
        }

        public string WriteMessageHint => MessageText.Length == 0 ? "Write a message..." : "";

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasMessages));
            }
        }

        private bool isMaximized = true;
        public bool IsMaximized
        {
            get => isMaximized;
            set { isMaximized = value; RaisePropertyChanged(); }
        }

        private bool isWindowMode;
        public bool IsWindowMode
        {
            get => isWindowMode;
            private set
            {
                isWindowMode = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanBeCollapsed));
            } 
        }

        public bool CanBeClosed => Conversation?.Id != Constants.PublicChannelId;

        public bool CanBeCollapsed => !IsWindowMode;

        public bool CanSendMessage => Conversation != null && !String.IsNullOrWhiteSpace(MessageText);

        public bool HasMessages => IsLoading || MessageViews?.Count > 0;

        public RelayCommand<object> CloseCommand { get; }
        public RelayCommand<object> SendMessageCommand { get; }
        public RelayCommand<object> ToggleMaximizedCommand { get; }
        public RelayCommand<object> ToggleWindowModeCommand { get; }

        public ChatBoxViewModel(IViewsManager viewsManager)
        {
            ViewsManager = viewsManager;

            CloseCommand = new RelayCommand<object>((_) => Close());
            SendMessageCommand = new RelayCommand<object>((_) => SendMessage());
            ToggleMaximizedCommand = new RelayCommand<object>((_) => ToggleMaximized());
            ToggleWindowModeCommand = new RelayCommand<object>((_) => ToggleWindowMode());

            MessageViews.CollectionChanged += (_, __) => RaisePropertyChanged(nameof(HasMessages));
        }

        public async void SetConversation(IConversation conversation)
        {
            IsLoading = true;
            Conversation = conversation;

            if (Conversation == null)
                return;

            var messages = await Conversation.GetMessages();

            foreach (var message in messages)
            {
                AddMessageView(message);
            }

            messages.CollectionChanged += (sender, args) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var newMessage in args.NewItems)
                    {
                        AddMessageView(newMessage as Message);
                    }
                    OnMessageReceived?.Invoke();
                });
            };

            IsLoading = false;
        }

        private void AddMessageView(Message message)
        {
            var messageView = ViewsManager.GetUserControl<ChatMessage>();
            messageView.ViewModel.Message = message;
            MessageViews.Add(messageView);
            RaisePropertyChanged(nameof(MessageViews));
        }

        private void Close()
        {
            OnClosed?.Invoke();
        }

        private void SendMessage()
        {
            if (!CanSendMessage)
                return;

            Conversation.SendMessage(MessageText);
            MessageText = "";
        }

        private void ToggleMaximized()
        {
            IsMaximized = !IsMaximized;
        }

        private void ToggleWindowMode()
        {
            IsWindowMode = !IsWindowMode;
            IsMaximized |= IsWindowMode;
            OnWindowModeToggled?.Invoke(IsWindowMode);
        }

        private static class Constants
        {
            public static readonly string PublicChannelId = "public";
        }
    }
}
