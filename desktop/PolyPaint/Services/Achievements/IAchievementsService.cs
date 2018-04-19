using System.Threading.Tasks;
using PolyPaint.Models;
using PolyPaint.Utils;

namespace PolyPaint.Services.Achievements
{
    public interface IAchievementsService
    {
        Task Increment(string userId, string metric);
        Task<ObservableDictionary<string, AchievementModel>> GetAchievements();
    }
}