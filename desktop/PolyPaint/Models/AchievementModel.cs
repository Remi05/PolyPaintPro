using Newtonsoft.Json;

namespace PolyPaint.Models
{
    public class AchievementModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("iconUri")]
        public string IconUri { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonIgnore]
        public bool Completed { get; set; } = false;
    }
}
