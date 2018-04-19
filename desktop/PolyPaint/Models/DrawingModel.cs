using Newtonsoft.Json;
using System.Collections.Generic;

namespace PolyPaint.Models
{
    public class DrawingModel
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }

        [JsonProperty("isProtected")]
        public bool IsProtected { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("selectedStrokes")]
        // Key: StrokeId, Value: UserID
        public Dictionary<string, string> SelectedStrokes { get; set; }
    }
}
