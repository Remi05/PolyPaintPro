using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PolyPaint.Services.Messaging
{
    public interface IMessagingService
    {
        Task<IConversation> CreateConversation(string conversationName);
        Task<ObservableCollection<IConversation>> GetConversations();
        Task<IConversation> GetPublicChannel();
    }
}
