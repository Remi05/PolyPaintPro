using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using PolyPaint.Models;

namespace PolyPaint.Services.Messaging
{
    public interface IConversation : INotifyPropertyChanged
    {
        string Id { get; }
        string Name { get; }

        Task<ObservableCollection<Message>> GetMessages();
        Task SendMessage(string text);
    }
}
