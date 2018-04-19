using Slofth.Firebase.Http;
using Moq;
using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;

namespace Slofth.Firebase.Tests.Http
{
    [TestFixture]
    class FirebaseErrorHandlingDecoratorTests
    {
        Mock<IFirebaseHttpClientFacade> BaseClientMock { get; set; }
        Mock<HttpResponseMessage> ResponseMock { get; set; }
        Mock<HttpContent> ContentMock { get; set; }
        IFirebaseHttpClientFacade ClientDecorator { get; set; }

        FirebaseAuthError Error { get; set; }
        bool IsSuccessStatusCode { get; set; }

        [SetUp]
        public void SetUp()
        {
            BaseClientMock = new Mock<IFirebaseHttpClientFacade>();
            ResponseMock = new Mock<HttpResponseMessage>();
            ContentMock = new Mock<HttpContent>();
            ClientDecorator = new FirebaseErrorHandlingDecorator<FirebaseAuthError>(BaseClientMock.Object);

            // TODO : We're going to have to somehow wrap the content mock, because we can't mock the ReadAsAsync extension method (because it's an extension method).
            // ContentMock.Setup(x => x.ReadAsAsync<Error>()).Returns(() => Task.FromResult(Error));

            ResponseMock.Setup(x => x.IsSuccessStatusCode).Returns(() => IsSuccessStatusCode);
            ResponseMock.Setup(x => x.Content).Returns(() => ContentMock.Object);

            BaseClientMock.Setup(x => x.GetAsync(It.IsAny<string>())).Returns(() => Task.FromResult(ResponseMock.Object));
            BaseClientMock.Setup(x => x.PutAsJsonAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(() => Task.FromResult(ResponseMock.Object));
            BaseClientMock.Setup(x => x.PostAsJsonAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(() => Task.FromResult(ResponseMock.Object));
            BaseClientMock.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(() => Task.FromResult(ResponseMock.Object));
        }

        class Constants
        {
            public static readonly string Url = "http://www.example.com/test";
        }
    }
}
