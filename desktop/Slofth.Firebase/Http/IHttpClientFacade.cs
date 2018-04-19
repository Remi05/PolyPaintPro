using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Slofth.Firebase.Http
{
    internal interface IFirebaseHttpClientFacade
    {
        HttpRequestHeaders Headers { get; }
        TimeSpan Timeout { get; set; }

        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value);
        Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value);
        Task<HttpResponseMessage> PatchAsJsonAsync<T>(string url, T value);
        Task<HttpResponseMessage> DeleteAsync(string url);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption);
    }
}