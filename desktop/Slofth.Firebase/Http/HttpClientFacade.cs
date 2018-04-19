using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Slofth.Firebase.Extensions;

namespace Slofth.Firebase.Http
{
    internal class HttpClientFacade : IFirebaseHttpClientFacade
    {
        private HttpClient Client { get; set; }

        public HttpRequestHeaders Headers => Client.DefaultRequestHeaders;

        public TimeSpan Timeout
        {
            get => Client.Timeout;
            set => Client.Timeout = value;
        }
        public HttpClientFacade()
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = true;
            Client = new HttpClient(httpClientHandler, true);
        }

        ~HttpClientFacade()
        {
            Client.Dispose();
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            return await Client.SendAsync(request, completionOption);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value)
        {
            return await Client.PostAsJsonAsync(url, value);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value)
        {
            return await Client.PutAsJsonAsync(url, value);
        }

        public async Task<HttpResponseMessage> PatchAsJsonAsync<T>(string url, T value)
        {
            return await Client.PatchAsJsonAsync(url, value);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await Client.GetAsync(url);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await Client.DeleteAsync(url);
        }
    }
}
