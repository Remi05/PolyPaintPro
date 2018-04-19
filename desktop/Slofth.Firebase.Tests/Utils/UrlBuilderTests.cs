using Slofth.Firebase.Utils;
using NUnit.Framework;
using System;

namespace Slofth.Firebase.Tests.Extensions
{
    [TestFixture]
    class UriBuilderExtensions
    {
        [Test]
        public void AddParam_StringParam_ShouldBuildExpectedUri()
        {
            // Arrange
            var baseUrl = "https://www.example.com";

            // Act
            var builder = UrlBuilder.Create(baseUrl).AddParam("my_param", "val");

            // Assert
            Assert.AreEqual("?my_param=val", builder.Query);
        }

        [Test]
        public void AddParam_MultipleParams_ShouldBuildExpectedUri()
        {
            // Arrange
            var baseUrl = "https://www.example.com";

            // Act
            var builder = UrlBuilder.Create(baseUrl).AddParam("my_param", "val").AddParam("my_other_param", 3);

            // Assert
            Assert.AreEqual("?my_param=val&my_other_param=3", builder.Query);
        }

        [Test]
        public void AddParam_IntParam_ShouldBuildExpectedUri()
        {
            // Arrange
            var baseUrl = "https://www.example.com";

            // Act
            var builder = UrlBuilder.Create(baseUrl).AddParam("my_param", 55);

            // Assert
            Assert.AreEqual("?my_param=55", builder.Query);
        }


        [Test]
        public void AddParam_ParamWithInvalidCharacters_ShouldEscapeKeyAndValue()
        {
            // Arrange
            var baseUrl = "https://www.example.com";

            // Act
            var builder = UrlBuilder.Create(baseUrl).AddParam("#myparam", "value$");

            // Assert
            Assert.AreEqual("?%23myparam=value%24", builder.Query);
        }

        [Test]
        public void AppendToPath_PathWithErraticSlashes_PathShouldBeSanitized()
        {
            // Arrange
            var baseUrl = "https://www.example.com";

            // Act
            var builder = UrlBuilder.Create(baseUrl)
                                    .AppendToPath("foo/")
                                    .AppendToPath("/bar///")
                                    .AppendToPath("lel");


            // Assert
            Assert.AreEqual("/foo/bar/lel", builder.Path);
        }

        [Test]
        public void Copy_ShouldProduceSameUrl()
        {
            // Arrange
            var baseUrl = "https://www.example.com";
            var originalBuilder = UrlBuilder.Create(baseUrl)
                                            .AppendToPath("foo/")
                                            .AppendToPath("/bar///")
                                            .AppendToPath("lel");

            // Act
            var copyBuilder = originalBuilder.Copy();

            // Assert
            Assert.AreEqual(originalBuilder.Url, copyBuilder.Url);

        }
    }
}
