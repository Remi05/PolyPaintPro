using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace PolyPaint.Models
{
    [JsonConverter(typeof(StrokeModelSerializer))]
    internal class StrokeModel : Stroke
    {
        public string Id { get; set; }
        public string AuthorId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModificationDate { get; set; }

        public StrokeModel(string id, string authorId, DateTime createdDate, StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(stylusPoints, drawingAttributes)
        {
            Id = id;
            AuthorId = authorId;
            CreatedDate = createdDate;
            LastModificationDate = createdDate;
        }

        public StrokeModel(string id, string authorId, DateTime createdDate, DateTime lastModificationDate, StylusPointCollection stylusPoints, DrawingAttributes drawingAttributes)
            : base(stylusPoints, drawingAttributes)
        {
            Id = id;
            AuthorId = authorId;
            CreatedDate = createdDate;
            LastModificationDate = lastModificationDate;
        }

        public StrokeModel(Stroke stroke, string authorId)
            : base(stroke.StylusPoints, stroke.DrawingAttributes)
        {
            Id = Guid.NewGuid().ToString();
            AuthorId = authorId;
            CreatedDate = DateTime.Now;
            LastModificationDate = DateTime.Now;
        }

        public class StrokeModelSerializer : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var stroke = value as StrokeModel;
                JObject jo = new JObject();
                jo.Add("id", stroke.Id);
                jo.Add("authorId", stroke.AuthorId);
                jo.Add("createdDate", stroke.CreatedDate);
                jo.Add("lastModificationDate", stroke.LastModificationDate);

                JObject attributes = new JObject();
                attributes.Add("color", JToken.FromObject(stroke.DrawingAttributes.Color));
                attributes.Add("width", JToken.FromObject(stroke.DrawingAttributes.Width));
                attributes.Add("height", JToken.FromObject(stroke.DrawingAttributes.Height));
                attributes.Add("stylusTip", JToken.FromObject(stroke.DrawingAttributes.StylusTip));
                jo.Add("drawingAttributes", attributes);

                var points = stroke.StylusPoints.Select(x => new { x.PressureFactor, x.X, x.Y, });
                jo.Add("stylusPoints", JToken.FromObject(points));

                jo.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jsonObject = JObject.Load(reader);
                var id = jsonObject["id"].ToObject<string>();
                var authorId = jsonObject["authorId"].ToObject<string>();
                var createdDate = jsonObject["createdDate"].ToObject<DateTime>();
                var lastModificationDate = jsonObject["lastModificationDate"].ToObject<DateTime>();
                var points = jsonObject["stylusPoints"].ToObject<StylusPointCollection>();
                var attributes = new DrawingAttributes
                {
                    Color = jsonObject["drawingAttributes"]["color"].ToObject<Color>(),
                    Width = jsonObject["drawingAttributes"]["width"].ToObject<double>(),
                    Height = jsonObject["drawingAttributes"]["height"].ToObject<double>(),
                    StylusTip = jsonObject["drawingAttributes"]["stylusTip"].ToObject<StylusTip>(),
                };

                return new StrokeModel(id, authorId, createdDate, lastModificationDate, points, attributes);
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(StrokeModel).IsAssignableFrom(objectType);
            }
        }
    }
}