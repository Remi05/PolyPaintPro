using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Slofth.Firebase.Tests")]
namespace Slofth.Firebase.Http
{
    class FirebaseErrorHandlingDecorator<TError> : HttpClientDecorator where TError : IFirebaseError, new()
    {
        public FirebaseErrorHandlingDecorator(IFirebaseHttpClientFacade baseComponent) : base(baseComponent) { }

        public override async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await ExecuteHandledRequest(async () => await BaseComponent.GetAsync(url));
        }

        public override async Task<HttpResponseMessage> PostAsJsonAsync<T>(string url, T value)
        {
            return await ExecuteHandledRequest(async () => await BaseComponent.PostAsJsonAsync(url, value));
        }

        public override async Task<HttpResponseMessage> PutAsJsonAsync<T>(string url, T value)
        {
            return await ExecuteHandledRequest(async () => await BaseComponent.PutAsJsonAsync(url, value));
        }

        public override async Task<HttpResponseMessage> PatchAsJsonAsync<T>(string url, T value)
        {
            return await ExecuteHandledRequest(async () => await BaseComponent.PatchAsJsonAsync(url, value));
        }

        public override async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await ExecuteHandledRequest(async () => await BaseComponent.DeleteAsync(url));
        }

        private async Task<HttpResponseMessage> ExecuteHandledRequest(Func<Task<HttpResponseMessage>> request)
        {
            var response = await request();
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsAsync<TError>();
                if (error != null)
                    throw error.GetCorrespondingException();

                throw new FirebaseException();
            }

            return response;
        }
    }
}
