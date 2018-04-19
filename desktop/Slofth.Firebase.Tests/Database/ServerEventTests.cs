using NUnit.Framework;
using Slofth.Firebase.Database;

namespace Slofth.Firebase.Tests.Database
{
    [TestFixture]
    class ServerEventTests
    {
        [Test]
        public void Parse_AuthRevoked_ShouldParseProperly()
        {
            // Act
            var parsedEvent = ServerEvent.Parse("event: auth_revoked", "data: sorry m8, you auth was revoked");

            // Assert
            Assert.AreEqual(parsedEvent.Type, ServerEventType.AuthRevoked);
            Assert.IsNull(parsedEvent.Data);
        }

        [Test]
        public void Parse_Cancel_ShouldParseProperly()
        {
            // Act
            var parsedEvent = ServerEvent.Parse("event: cancel", "null");

            // Assert
            Assert.AreEqual(parsedEvent.Type, ServerEventType.Cancel);
            Assert.IsNull(parsedEvent.Data);
        }

        [Test]
        public void Parse_KeepAlive_ShouldParseProperly()
        {
            // Act
            var parsedEvent = ServerEvent.Parse("event: keep-alive", "null");

            // Assert
            Assert.AreEqual(parsedEvent.Type, ServerEventType.KeepAlive);
            Assert.IsNull(parsedEvent.Data);
        }
    }
}
