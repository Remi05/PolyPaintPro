using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Slofth.Firebase.Database
{
    class ServerEvent
    {
        public ServerEventType Type { get; private set; }
        public string Path { get; private set; }
        public string ChildKey => String.IsNullOrEmpty(Path) ? "" : Path.Split('.')[0];
        public JToken Data { get; private set; }

        private ServerEvent(ServerEventType eventType, string path, JToken data)
        {
            Type = eventType;
            Path = path;
            Data = data;
        }

        internal static ServerEvent Parse(string serializedEvent, string serializedData)
        {
            if (serializedEvent == null || serializedData == null) return null;

            var type = ToDatabaseEvent(serializedEvent);

            if (type == ServerEventType.Put || type == ServerEventType.Patch)
            {
                // The data part looks like this: data: { path: "path/to/resource", data: {data: 2, otherData: "helo" }}

                // We need to separate the keys with dots instead of slashes.
                // The data is a json object. It is null if we deal with a remove operation.
                PathDataPair content = JsonConvert.DeserializeObject<PathDataPair>(serializedData.Split(new char[] { ':' }, 2)[1]);

                return new ServerEvent(type, content?.Path, content?.Data);
            }
            else
            {
                return new ServerEvent(type, null, null);
            }
        }

        private static ServerEventType ToDatabaseEvent(string eventName)
        {
            var sauce = eventName.Split(':')[1].Trim();

            switch (sauce)
            {
                case "put":
                    return ServerEventType.Put;

                case "patch":
                    return ServerEventType.Patch;

                case "keep-alive":
                    return ServerEventType.KeepAlive;

                case "auth_revoked":
                    return ServerEventType.AuthRevoked;

                case "cancel":
                    return ServerEventType.Cancel;

                default:
                    throw new UnknownServerEventException();
            }
        }
    }

    class PathDataPair
    {
        private string path;
        [JsonProperty("path")]
        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value?.Replace('/', '.')?.TrimStart('.');
            }
        }

        [JsonProperty("data")]
        public JToken Data { get; set; }
    }
}
