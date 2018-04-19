using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PolyPaint.Models
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class DrawingInfo
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("previewUrl")]
        public string PreviewUrl { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("tags")]
        public IList<string> Tags { get; set; }

        [JsonProperty("likes")]
        public Dictionary<string, bool> Likes { get; set; }

        [JsonProperty("reports")]
        public Dictionary<string, string> Reports { get; set; }

        [JsonProperty("last_modified_on")]
        public DateTime LastModifiedOn { get; set; }

        [JsonProperty("nsfw")]
        public bool IsNsfw { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("emailSent")]
        public bool EmailSent { get; set; }

        [JsonIgnore]
        public bool IsBanned => Reports != null && Reports.Count >= 3;

        public void Copy(DrawingInfo other)
        {
            Id = other.Id;
            PreviewUrl = other.PreviewUrl;
            Tags = other.Tags;
            Likes = other.Likes;
            Reports = other.Reports;
            LastModifiedOn = other.LastModifiedOn;
            IsNsfw = other.IsNsfw;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DrawingInfo;
            return other != null
                && other.Id == Id
                && other.PreviewUrl == PreviewUrl
                && (other.Tags == null && Tags == null || other.Tags != null && Tags != null && other.Tags.SequenceEqual(Tags))
                && (other.Likes == null && Likes == null || Likes != null && other.Likes != null && other.Likes.SequenceEqual(Likes))
                && (other.Reports == null && Reports == null || Reports != null && other.Reports != null && other.Reports.SequenceEqual(Reports))
                && other.LastModifiedOn == LastModifiedOn
                && other.IsNsfw == IsNsfw
                && other.EmailSent == EmailSent;
        }
    }
}
