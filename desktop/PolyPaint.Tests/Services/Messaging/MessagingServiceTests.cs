using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PolyPaint.Models;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Messaging;
using PolyPaint.Services.Social;
using PolyPaint.Tests.Services.Database;
using PolyPaint.Utils;

namespace PolyPaint.Tests.Services.Messaging
{
    [TestFixture]
    class MessagingServiceTests
    {
        private Mock<IAuthenticationService> AuthServiceMock { get; set; }
        private MockDatabaseService DatabaseServiceMock { get; set; }
        private Mock<ILogger> LoggerMock { get; }
        private IMessagingService MessagingService { get; set; }
        private Mock<IProfileService> ProfileServiceMock { get; set; }

        [SetUp]
        public void SetUp()
        {
            DatabaseServiceMock = new MockDatabaseService();

            AuthServiceMock = new Mock<IAuthenticationService>();
            AuthServiceMock.Setup(x => x.CurrentUser).Returns(Constants.User1);
            ProfileServiceMock = new Mock<IProfileService>();
            MessagingService = new MessagingService(AuthServiceMock.Object, DatabaseServiceMock, LoggerMock.Object, ProfileServiceMock.Object);
        }

        [Test]
        public async Task CreateConversation_ShouldCreateConversationAndAddIdToMembersUserData()
        {
            // Arrange
            string expectedName = Constants.ConversationInfo1.Name;

            AuthServiceMock.Setup(authService => authService.CurrentUser).Returns(Constants.User1);

            // Act
            await MessagingService.CreateConversation(expectedName);

            // Assert
            // Verify that one (and only one) conversation with the correct info is added to the database.
            var conversations = await DatabaseServiceMock.Ref(DatabasePaths.Conversations)
                                                         .Once<Dictionary<string, ConversationModel>>();

            var conversation = conversations.First().Value;

            Assert.AreEqual(conversations.Count, 1);
            Assert.IsNotNull(conversation);
        }

        [Test]
        public async Task GetConversations_ShouldReturnExistingConversations()
        {
            // Arrange
            User user = Constants.User1;
            var expectedConversationInfos = new Dictionary<string, ConversationModel>() {
                { Constants.ConversationInfo1.Id, Constants.ConversationInfo1 },
                { Constants.ConversationInfo2.Id, Constants.ConversationInfo2 }
            };

            var expectedUserConversations = new Dictionary<string, string>();
            foreach (var conversationId in expectedConversationInfos.Keys)
            {
                expectedUserConversations.Add(conversationId, conversationId);
            }

            AuthServiceMock.Setup(authService => authService.CurrentUser).Returns(user);

            await DatabaseServiceMock.Ref(DatabasePaths.Users)
                                     .Child(user.Id)
                                     .Child(DatabasePaths.Conversations)
                                     .Set(expectedUserConversations);

            await DatabaseServiceMock.Ref(DatabasePaths.Conversations)
                                     .Set(expectedConversationInfos);

            // Act
            var actualConversations = await MessagingService.GetConversations();

            // Assert
            var expectedConversations = expectedConversationInfos.Values.Select(info => new Conversation(info, AuthServiceMock.Object, DatabaseServiceMock));
            Assert.AreEqual(expectedConversations.Count(), actualConversations.Count());
            foreach (var expectedConversation in expectedConversations)
            {
                Assert.IsTrue(actualConversations.Contains(expectedConversation));
            }
        }

        private static class Constants
        {
            public static readonly string UserId1 = "testUserId1";
            public static readonly string UserId2 = "testUserId2";
            public static readonly string DisplayName1 = "testDisplayName1";
            public static readonly string DisplayName2 = "testDisplayName2";
            public static readonly string Email1 = "dude1@test.com";
            public static readonly string Email2 = "dude2@test.com";
            public static readonly string PhotoUrl1 = "www.test.com/dude1.jpeg";
            public static readonly string PhotoUrl2 = "www.test.com/dude2.jpeg";
            public static readonly User User1 = new User(UserId1, DisplayName1, Email1, PhotoUrl1);
            public static readonly User User2 = new User(UserId2, DisplayName2, Email2, PhotoUrl2);
            public static readonly ConversationModel ConversationInfo1 = new ConversationModel("0", "convo1");
            public static readonly ConversationModel ConversationInfo2 = new ConversationModel("1", "convo2");
        }
    }
}
