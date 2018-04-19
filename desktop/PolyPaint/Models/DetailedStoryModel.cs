using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace PolyPaint.Models
{
    public class DetailedStoryModel : StoryModel
    {
        [JsonIgnore]
        public ProfileModel Owner { get; set; }

        [JsonIgnore]
        public List<string> DrawingPreviewUrls { get; set; }
    }
}
