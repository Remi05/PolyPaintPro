using Newtonsoft.Json;
using System;

namespace PolyPaint.Models
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class Message
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        [JsonProperty("text")]
        public string Text { get; private set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; private set; }

        [JsonProperty("senderId")]
        public string SenderId { get; private set; }

        [JsonProperty("senderName")]
        public string SenderName { get; set; }

        [JsonIgnore]
        public bool WasSentByCurrentUser { get; set; }
             
        public Message(string text, DateTime timestamp, string senderId, string senderName)
        {
            Text = text;
            Timestamp = timestamp;
            SenderId = senderId;
            SenderName = senderName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Message))
                return false;

            var otherMessage = obj as Message;
            return otherMessage == this
                || otherMessage.Text.Equals(Text)
                && otherMessage.Timestamp.Equals(Timestamp)
                && otherMessage.SenderId.Equals(SenderId);
        }
    }
}
