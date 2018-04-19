using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PolyPaint.Models;

namespace PolyPaint.Services.Stories
{
    public interface IStoriesService
    {
        Task AppendToStory(string drawingId);
        Task RemoveFromStory(string drawingId);
        Task<StoryModel> GetStory(string userId);
        Task<ObservableCollection<DetailedStoryModel>> GetStories();
    }
}