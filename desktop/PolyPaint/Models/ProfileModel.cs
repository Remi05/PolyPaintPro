using Newtonsoft.Json;

namespace PolyPaint.Models
{
    public class ProfileModel
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("hasSeenTutorial")]
        public bool? HasSeenTutorial { get; set; }

        public ProfileModel(string displayName, string photoUrl, string userId, bool? hasSeenTutorial = null)
        {
            DisplayName = displayName;
            PhotoUrl = photoUrl;
            UserId = userId;
            HasSeenTutorial = hasSeenTutorial;
        }
    }
}
