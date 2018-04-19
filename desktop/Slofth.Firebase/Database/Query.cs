using Slofth.Firebase.Http;
using Slofth.Firebase.Utils;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Slofth.Firebase.Database
{
    public abstract partial class Query
    {
        internal IFirebaseHttpClientFacade Client { get; set; }
        internal UrlBuilder UrlBuilder { get; set; }
        protected Func<Task<string>> IdTokenFactory { get; set; }

        public string Key { get; private set; }

        internal Query(UrlBuilder urlBuilder, string name, Func<Task<string>> idTokenFactory)
        {
            UrlBuilder = urlBuilder;
            Key = name;
            IdTokenFactory = idTokenFactory;
            Client = FirebaseHttpClientFactory.CreateFirebaseDatabaseHttpClient();
        }

        /// <summary>
        /// Reads data stored at the specified reference.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <returns>The deserialized data.</returns>
        public async virtual Task<T> Once<T>()
        {
            UrlBuilder.AppendToPath(Endpoints.Json).AddParam(Params.Auth, await IdTokenFactory());
            var response = await Client.GetAsync(UrlBuilder.Url);

            try
            {
                return await response.Content.ReadAsAsync<T>();
            }
            catch (JsonSerializationException) { } // Means the content is null

            return default(T);
        }

        public virtual Subscription OnChildAdded<T>(Action<T> callback)
        {
            return FirebaseObservable.ListenChildAdded(UrlBuilder, IdTokenFactory, callback);
        }

        public virtual Subscription OnChildChanged<T>(Action<T> callback)
        {
            return FirebaseObservable.ListenChildChanged(UrlBuilder, IdTokenFactory, callback);
        }

        public virtual Subscription OnChildRemoved<T>(Action<T> callback)
        {
            return FirebaseObservable.ListenChildRemoved<T>(UrlBuilder, IdTokenFactory, callback);
        }

        public virtual Subscription OnValue<T>(Action<T> callback)
        {
            return FirebaseObservable.ListenValue(UrlBuilder, IdTokenFactory, callback);
        }

        /// <summary>
        /// Saves data to the current reference. Replaces any data at that path.
        /// </summary>
        public async Task Set<T>(T value)
        {
            UrlBuilder.AppendToPath(Endpoints.Json).AddParam(Params.Auth, await IdTokenFactory());
            var response = await Client.PutAsJsonAsync(UrlBuilder.Url, value);
        }

        public async Task Update<T>(T value)
        {
            UrlBuilder.AppendToPath(Endpoints.Json).AddParam(Params.Auth, await IdTokenFactory());
            var response = await Client.PatchAsJsonAsync(UrlBuilder.Url, value);
        }

        public ChildQuery Child(string name)
        {
            return new ChildQuery(UrlBuilder.AppendToPath(name), name, IdTokenFactory);
        }

        public async Task<ChildQuery> Push()
        {
            var builderCopy = UrlBuilder.Copy().AppendToPath(Endpoints.Json).AddParam(Params.Auth, await IdTokenFactory());
            var response = await Client.PostAsJsonAsync(builderCopy.Url, new { });

            var content = await response.Content.ReadAsAsync<PostInfo>();
            return Child(content.Name);
        }

        /// <summary>
        /// Deletes the data at the specified reference.
        /// </summary>
        /// <returns></returns>
        public async Task Remove()
        {
            UrlBuilder.AppendToPath(Endpoints.Json).AddParam(Params.Auth, await IdTokenFactory());
            var response = await Client.DeleteAsync(UrlBuilder.Url);
        }

        protected string Quote(string text)
        {
            return $"\"{text}\"";
        }

        static class Endpoints
        {
            public static readonly string Json = ".json";
        }

        static class Params
        {
            public static readonly string Auth = "auth";
        }
    }
}
