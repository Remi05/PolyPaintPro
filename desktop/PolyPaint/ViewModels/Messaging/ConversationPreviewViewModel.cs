using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolyPaint.Services.Messaging;
using PolyPaint.Utils;

namespace PolyPaint.ViewModels.Messaging
{
    public interface IConversationPreviewViewModel : IViewModel
    {
        event Action<IConversation> OnClicked;

        IConversation Conversation { get; set; }

        RelayCommand<object> ClickCommand { get; }
    }

    public class ConversationPreviewViewModel : ViewModel, IConversationPreviewViewModel
    {
        public event Action<IConversation> OnClicked;

        private IConversation conversation;
        public IConversation Conversation
        {
            get => conversation;
            set { conversation = value; RaisePropertyChanged(); }
        }

        public RelayCommand<object> ClickCommand { get; }

        public ConversationPreviewViewModel()
        {
            ClickCommand = new RelayCommand<object>((_) => Click());
        }

        private void Click()
        {
            OnClicked?.Invoke(Conversation);
        }
    }
}
