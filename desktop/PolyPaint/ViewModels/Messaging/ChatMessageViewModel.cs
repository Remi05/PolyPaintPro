using System.Windows;
using System.Windows.Media;
using PolyPaint.Models;

namespace PolyPaint.ViewModels.Messaging
{
    public interface IChatMessageViewModel : IViewModel
    {
        Message Message { get; set; }
    }

    public class ChatMessageViewModel : ViewModel, IChatMessageViewModel
    {
        private Message message;
        public Message Message
        {
            get => message;
            set { message = value; RaisePropertyChanged(); }
        }
    }
}
