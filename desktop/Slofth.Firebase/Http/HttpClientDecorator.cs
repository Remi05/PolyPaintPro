using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Slofth.Firebase.Http
{
    internal abstract class HttpClientDecorator : IFirebaseHttpClientFacade
    {
        public HttpRequestHeaders Headers => BaseComponent.Headers;
        public TimeSpan Timeout
        {
            get => BaseComponent.Timeout;
            set => BaseComponent.Timeout = value;
        }

        protected IFirebaseHttpClientFacade BaseComponent { get; set; }

        protected HttpClientDecorator(IFirebaseHttpClientFacade baseComponent)
        {
            BaseComponent = baseComponent;
        }

        public abstract Task<HttpResponseMessage> GetAsync(string url);
        public abstract Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value);
        public abstract Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value);
        public abstract Task<HttpResponseMessage> PatchAsJsonAsync<T>(string url, T value);
        public abstract Task<HttpResponseMessage> DeleteAsync(string url);
        
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            return await BaseComponent.SendAsync(request, completionOption);
        }

    }
}
