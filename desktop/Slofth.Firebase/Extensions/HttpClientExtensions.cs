using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Slofth.Firebase.Extensions
{
    internal static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var method = new HttpMethod("PATCH");
            var serializedData = JsonConvert.SerializeObject(value);
            var content = new StringContent(serializedData, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(method, requestUri)
            {
                Content = content
            };

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                response = await client.SendAsync(request);
            }
            catch (TaskCanceledException) { }

            return response;
        }
    }
}
