using Newtonsoft.Json;

namespace PolyPaint.Models
{
    public class Profile
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
        [JsonProperty("photoUrl")]
        public string PhotoUrl { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
        public Profile(string displayName, string photoUrl, string userId)
        {
            DisplayName = displayName;
            PhotoUrl = photoUrl;
            UserId = userId;
            
        }
    }
}
