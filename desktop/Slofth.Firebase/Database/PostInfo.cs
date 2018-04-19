using Newtonsoft.Json;

namespace Slofth.Firebase.Database
{
    public abstract partial class Query
    {
        class PostInfo
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
