using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Messaging;
using PolyPaint.Tests.Services.Database;

namespace PolyPaint.Tests.Services.Messaging
{
    [TestFixture]
    class ConversationTests
    {
        private Mock<IAuthenticationService> AuthServiceMock { get; set; }
        private MockDatabaseService DatabaseServiceMock { get; set; }
        private IConversation Conversation { get; set; }
        private User CurrentUser { get; set; }
        [SetUp]
        public void SetUp()
        {
            CurrentUser = new User(Constants.UserId1, "Testy", "myemail@example.com", null);
            AuthServiceMock = new Mock<IAuthenticationService>();
            AuthServiceMock.Setup(x => x.CurrentUser).Returns(CurrentUser);
            DatabaseServiceMock = new MockDatabaseService();
        }

        [Test]
        [Ignore("This isn't the expected behavior anymore.")]
        public async Task GetMessages_ShouldReturnExistingMessages()
        {
            // Arrange
            Conversation = new Conversation(Constants.ConversationInfo, AuthServiceMock.Object, DatabaseServiceMock);

            var messages = new Dictionary<string, Message>() {
                { "0987654321", new Message("allo", DateTime.Now, Constants.UserId1, "remi") },
                { "1234567890", new Message("sup?", DateTime.Now, Constants.UserId2, "bob") },
                { "1029384756", new Message("just testing stuff", DateTime.Now, Constants.UserId1, "remi") },
            };

            await DatabaseServiceMock.Ref(Constants.MessagesCollectionName)
                                     .Child(Conversation.Id)
                                     .Set(messages);

            // Act
            var actualMessages = await Conversation.GetMessages();

            // Assert
            Assert.AreEqual(messages.Count(), actualMessages.Count());
            foreach (var expectedMessage in messages.Values)
            {
                Assert.IsTrue(actualMessages.Contains(expectedMessage));
            }
        }

        [Test, Timeout(6000)]
        public async Task SendMessage_ShouldAddNewMessageToConversation()
        {
            // Arrange
            Conversation = new Conversation(Constants.ConversationInfo, AuthServiceMock.Object, DatabaseServiceMock);
            string expectedText = "test";
            string expectedUserId = Constants.UserId1;

            // Act
            await Conversation.SendMessage(expectedText);

            // Assert
            var messages = await DatabaseServiceMock.Ref(Constants.MessagesCollectionName)
                                                    .Child(Conversation.Id)
                                                    .Once<Dictionary<string, Message>>();
            var macthingMessages = messages.Select(kvp => kvp.Value)
                                         .Where(message =>
                                         {
                                             return message.Text.Equals(expectedText)
                                                 && message.SenderId.Equals(expectedUserId);
                                         });

            Assert.AreEqual(1, macthingMessages.Count());
        }

        private static class Constants
        {
            public static readonly string MessagesCollectionName = "messages";
            public static readonly string ConversationId = "convo1";
            public static readonly string ConversationName = "test convo";
            public static readonly string UserId1 = "testy";
            public static readonly string UserId2 = "bob";
            public static readonly Dictionary<string, bool> ConversationMembers = new Dictionary<string, bool>() { { UserId1, true }, { UserId2, true } };
            public static readonly ConversationModel ConversationInfo = new ConversationModel(ConversationId, ConversationName);
        }
    }
}
