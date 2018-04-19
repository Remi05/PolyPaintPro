using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyPaint.Models
{
    class UserModel
    {
        [JsonProperty("conversations")]
        public Dictionary<string, string> Conversations { get; set; }

        [JsonProperty("drawings")]
        public Dictionary<string, string> Drawings { get; set; }

        [JsonProperty("didTutorial")]
        public bool DidTutorial { get; set; }

        [JsonProperty("followings")]
        public Dictionary<string, bool> Followings { get; set; }

        [JsonProperty("followers")]
        public Dictionary<string, string> Followers { get; set; }

        [JsonProperty("profile")]
        public ProfileModel Profile { get; set; }
    }
}
