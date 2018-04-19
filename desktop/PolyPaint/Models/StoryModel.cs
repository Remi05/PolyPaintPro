using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PolyPaint.Models
{
    public class StoryModel
    {
        [JsonProperty("expirationDate")]
        public DateTime ExpirationDate { get; set; }

        [JsonProperty("drawings")]
        public Dictionary<string, int> Drawings { get; set; }

        [JsonIgnore]
        public bool IsExpired => ExpirationDate < DateTime.Now;
    }
}
