using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PolyPaint.Services;
using PolyPaint.Services.Messaging;
using PolyPaint.Utils;
using PolyPaint.Views.Messaging;

namespace PolyPaint.ViewModels.Messaging
{
    public interface IConversationsListViewModel : IViewModel
    {
        event Action<IConversation> OnConversationSelected;

        IEnumerable<ConversationPreview> FilteredConversations { get; }
        string ConversationName { get; }
        string HintMessage { get; }
        string ConversationCreationText { get; }
        bool CanCreateConversation { get; }
        bool IsSearchingConversation { get; }
        bool IsLoading { get; }
        bool IsMaximized { get; }

        RelayCommand<object> CreateOrJoinConversationCommand { get; }
        RelayCommand<object> ToggleMaximizedCommand { get; }
        RelayCommand<object> ToggleSearchingConversationCommand { get; }
    }

    public class ConversationsListViewModel : ViewModel, IConversationsListViewModel
    {
        public event Action<IConversation> OnConversationSelected;

        private IMessagingService MessagingService { get; }
        private IViewsManager ViewsManager { get; }

        private ObservableCollection<ConversationPreview> conversationPreviews = new ObservableCollection<ConversationPreview>();
        private ObservableCollection<ConversationPreview> ConversationPreviews
        {
            get => conversationPreviews;
            set
            {
                conversationPreviews = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(FilteredConversations));
                RaisePropertyChanged(nameof(CanCreateConversation));
            }
        }

        public IEnumerable<ConversationPreview> FilteredConversations => ConversationPreviews.Where(IsMatchingConversation);

        private string conversationName = "";
        public string ConversationName
        {
            get => conversationName;
            set
            {
                conversationName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HintMessage));
                RaisePropertyChanged(nameof(FilteredConversations));
                RaisePropertyChanged(nameof(CanCreateConversation));
                RaisePropertyChanged(nameof(ConversationCreationText));
            }
        }

        public string HintMessage => ConversationName.Length == 0 ? "Enter a channel name..." : "";

        public string ConversationCreationText => $"Create channel \"{ConversationName}\"";

        public bool CanCreateConversation => IsSearchingConversation 
                                          && ConversationName.Length > 0 
                                          && !ConversationPreviews.Any(preview => preview.ViewModel.Conversation.Name == ConversationName);

        private bool isSearchingConversation;
        public bool IsSearchingConversation
        {
            get => isSearchingConversation;
            private set
            {
                isSearchingConversation = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CanCreateConversation));
            }
        }

        private bool isLoading = true;
        public bool IsLoading
        {
            get => isLoading;
            private set { isLoading = value; RaisePropertyChanged(); }
        }

        private bool isMaximized;
        public bool IsMaximized
        {
            get => isMaximized;
            private set { isMaximized = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> CreateOrJoinConversationCommand { get; }
        public RelayCommand<object> ToggleMaximizedCommand { get; }
        public RelayCommand<object> ToggleSearchingConversationCommand { get; }

        public ConversationsListViewModel(IMessagingService messagingService, IViewsManager viewsManager)
        {
            MessagingService = messagingService;
            ViewsManager = viewsManager;

            CreateOrJoinConversationCommand = new RelayCommand<object>((_) => CreateOrJoinConversation());
            ToggleMaximizedCommand = new RelayCommand<object>((_) => ToggleMaximized());
            ToggleSearchingConversationCommand = new RelayCommand<object>((_) => ToggleSearchingConversation());

            LoadConversations();
        }

        private async void LoadConversations()
        {
            var conversations = await MessagingService.GetConversations();

            foreach (var conversation in conversations)
            {
                AddConversation(conversation);
            }

            IsLoading = false;

            conversations.CollectionChanged += (sender, args) =>
            {
                if (args?.NewItems == null)
                    return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var newConversation in args.NewItems)
                    {
                        if (newConversation != null)
                        {
                            AddConversation(newConversation as IConversation);
                            RaisePropertyChanged(nameof(FilteredConversations));
                        }
                    }
                });
            };
        }

        private void AddConversation(IConversation conversation)
        {
            var conversationPreview = ViewsManager.GetUserControl<ConversationPreview>();
            conversationPreview.ViewModel.Conversation = conversation;
            conversationPreview.ViewModel.OnClicked += (IConversation c) => OnConversationSelected?.Invoke(c);
            ConversationPreviews.Add(conversationPreview);
        }

        private async void CreateOrJoinConversation()
        {
            if (!IsSearchingConversation)
                return;

            IsSearchingConversation = false;

            var conversationPreview = ConversationPreviews.FirstOrDefault(preview => preview.ViewModel.Conversation.Name == ConversationName);

            var conversation = conversationPreview != null 
                             ? conversationPreview.ViewModel.Conversation
                             : await MessagingService.CreateConversation(ConversationName);

            OnConversationSelected?.Invoke(conversation);
            ConversationName = "";
        }

        private void ToggleMaximized()
        {
            IsMaximized = !IsMaximized;
            IsSearchingConversation &= IsMaximized;
        }

        private void ToggleSearchingConversation()
        {
            IsSearchingConversation = !IsSearchingConversation;
            IsMaximized |= IsSearchingConversation;
        }

        private bool IsMatchingConversation(ConversationPreview conversationPreview)
        {
            string name = conversationPreview.ViewModel.Conversation.Name;
            return !IsSearchingConversation
                || string.IsNullOrWhiteSpace(ConversationName) 
                || name.ToLower().Contains(ConversationName.ToLower());
        }
    }
}
