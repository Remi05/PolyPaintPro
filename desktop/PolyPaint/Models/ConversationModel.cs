using Newtonsoft.Json;

namespace PolyPaint.Models
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class ConversationModel
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {   
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public ConversationModel(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ConversationModel))
                return false;

            var otherInfo = obj as ConversationModel;

            return otherInfo == this
                || otherInfo.Id.Equals(Id)
                && otherInfo.Name.Equals(Name);
        }
    }
}
