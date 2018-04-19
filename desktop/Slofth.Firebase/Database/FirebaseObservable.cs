using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Slofth.Firebase.Http;
using Slofth.Firebase.Utils;
using System.Linq;
using System.Threading;

namespace Slofth.Firebase.Database
{
    public delegate void DatabaseEventHandler(JToken o);

    internal enum ServerEventType
    {
        Put, Patch, KeepAlive, Cancel, AuthRevoked
    }

    public class FirebaseObservable
    {
        private CancellationTokenSource CancellationTokenSource { get; set; } = new CancellationTokenSource();
        private CancellationToken CancellationToken { get; set; }

        // TODO : Handle ChildMoved event
        // public event DatabaseEventHandler<T> ChildMoved; 
        private event DatabaseEventHandler ValueChanged;
        private event DatabaseEventHandler ChildAdded;
        private event DatabaseEventHandler ChildChanged;
        private event DatabaseEventHandler ChildRemoved;

        private JObject Cache { get; set; }

        private Func<Task<string>> IdTokenFactory { get; set; }
        private UrlBuilder UrlBuilder { get; set; }
        private IFirebaseHttpClientFacade Client { get; set; }

        private object CacheLock { get; set; } = new object();

        private static FirebaseObservable Create(UrlBuilder urlBuilder, Func<Task<string>> idTokenFactory)
        {
            FirebaseObservable subscription;

            subscription = new FirebaseObservable();
            subscription.UrlBuilder = urlBuilder;
            subscription.IdTokenFactory = idTokenFactory;
            subscription.Client = FirebaseHttpClientFactory.CreateFirebaseDatabaseHttpClient();
            subscription.Client.Timeout = Constants.Timeout;
            subscription.Cache = new JObject();

            return subscription;
        }

        internal static Subscription ListenChildAdded<T>(UrlBuilder urlBuilder, Func<Task<string>> idTokenFactory, Action<T> callback)
        {
            var observable = Create(urlBuilder, idTokenFactory);
            observable.ChildAdded += (token) => callback(token.ToObject<T>());
            observable.Start();
            return new Subscription(observable);
        }

        internal static Subscription ListenChildChanged<T>(UrlBuilder urlBuilder, Func<Task<string>> idTokenFactory, Action<T> callback)
        {
            var observable = Create(urlBuilder, idTokenFactory);
            observable.ChildChanged += (token) => callback(token.ToObject<T>());
            observable.Start();
            return new Subscription(observable);
        }

        internal static Subscription ListenChildRemoved<T>(UrlBuilder urlBuilder, Func<Task<string>> idTokenFactory, Action<T> callback)
        {
            var observable = Create(urlBuilder, idTokenFactory);
            observable.ChildRemoved += (token) => callback(token.ToObject<T>());
            observable.Start();
            return new Subscription(observable);
        }

        internal static Subscription ListenValue<T>(UrlBuilder urlBuilder, Func<Task<string>> idTokenFactory, Action<T> callback)
        {
            var observable = Create(urlBuilder, idTokenFactory);
            observable.ValueChanged += (token) =>
            {
                T obj;
                try
                {
                    obj = token.ToObject<T>();
                }
                catch (Exception)
                {
                    Console.WriteLine($"[FirebaseObservable Error] An error occured while deserializing. Request:  {urlBuilder.ToString()}");
                    obj = default(T);
                }

                callback(obj);
            };

            observable.Start();
            return new Subscription(observable);
        }

        private void Start()
        {
            CancellationToken = CancellationTokenSource.Token;
            Task.Run(ListenToServerEvents, CancellationToken);
        }

        internal void Stop()
        {
            CancellationTokenSource.Cancel();
        }

        private async Task ListenToServerEvents()
        {
            UrlBuilder.AppendToPath(Endpoints.Json);

            while (!CancellationToken.IsCancellationRequested)
            {
                var urlBuilderCopy = UrlBuilder.Copy();
                urlBuilderCopy.AddParam(Params.Auth, await IdTokenFactory());

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(urlBuilderCopy.Url));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                HttpResponseMessage response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                using (Stream stream = await response.Content.ReadAsStreamAsync())
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!CancellationToken.IsCancellationRequested)
                    {
                        var serializedEventType = await reader.ReadLineAsync();
                        if (String.IsNullOrWhiteSpace(serializedEventType)) continue;

                        string serializedData = await reader.ReadLineAsync();

                        ServerEvent serverEvent = ServerEvent.Parse(serializedEventType, serializedData);
                        if (serverEvent.Type == ServerEventType.AuthRevoked) { break; }
                        if (serverEvent.Type == ServerEventType.KeepAlive) { continue; }
                        if (serverEvent.Type == ServerEventType.Cancel) { throw new PremissionDeniedException(); }

                        if (serverEvent.Type == ServerEventType.Put) { HandlePut(serverEvent); }
                        else if (serverEvent.Type == ServerEventType.Patch) { HandlePatch(serverEvent); }
                    }
                }
            }

            // We clear all the events
            ValueChanged = ChildAdded = ChildChanged = ChildRemoved = null;
        }

        private void HandlePut(ServerEvent serverEvent)
        {
            lock (CacheLock)
            {
                var token = Cache.SelectToken(serverEvent.ChildKey, false);
                if (token != null && token.Parent == null)
                {
                    var newChildren = serverEvent.Data.Children().Except(Cache.Children());
                    foreach (var child in newChildren)
                    {
                        ChildAdded?.Invoke(child.GetType() == typeof(JValue) ? child : child.First);
                    }

                    var removedChildren = Cache.Children().Except(serverEvent.Data.Children());
                    foreach (var child in removedChildren) { ChildRemoved?.Invoke(child.First); }

                    Cache = serverEvent.Data as JObject ?? new JObject();
                }
                else
                {
                    if (token == null)
                    {
                        Cache[serverEvent.Path] = serverEvent.Data as JToken;
                        ChildAdded?.Invoke(Cache[serverEvent.Path]);
                    }
                    else
                    {
                        if (serverEvent.Data.ToObject<object>() == null)
                        {
                            var removedChild = Cache.SelectToken(serverEvent.Path, false);
                            Cache.SelectToken(serverEvent.Path, false)?.Parent?.Remove();
                            ChildRemoved?.Invoke(removedChild);
                        }
                        else
                        {
                            var subChildPath = string.Join(".", serverEvent.Path.Split('.').Skip(1));
                            token[subChildPath] = serverEvent.Data as JToken;
                            ChildChanged?.Invoke(Cache.SelectToken(serverEvent.ChildKey, false));
                        }
                    }
                }

                ValueChanged?.Invoke(Cache);
            }
        }

        private void HandlePatch(ServerEvent serverEvent)
        {
            lock (CacheLock)
            {
                var dest = Cache.SelectToken(serverEvent.Path);
                if (dest == null)
                {
                    dest = new JObject();
                    Cache[serverEvent.Path] = dest;
                }
                foreach (var key in serverEvent.Data)
                {
                    if (key.First.ToObject<object>() != null)
                    {
                        dest[key.Path] = key.First;
                    }
                    else
                    {
                        (dest as JObject).Remove(key.Path);
                    }
                }

                ValueChanged?.Invoke(Cache);
            }
        }

        class Constants
        {
            // Arbitrarily long timeout; we don't want the connection to stop. 
            public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5200);
        }

        class Endpoints
        {
            public static readonly string Json = ".json";
        }

        class Params
        {
            public static readonly string Auth = "auth";
        }
    }
}