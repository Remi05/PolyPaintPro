using System;
using System.Threading.Tasks;
using NUnit.Framework;
using PolyPaint.Services.Auth;

namespace PolyPaint.Tests.Services
{
    [TestFixture]
    internal class FirebaseAuthenticationServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            FirebaseHelper = new FirebaseHelper();
            AuthService = new FirebaseAuthenticationService();
        }

        [TearDown]
        [OneTimeTearDown]
        public async Task TearDown()
        {
            await FirebaseHelper.CleanUp();
        }

        private FirebaseHelper FirebaseHelper { get; set; }
        private IAuthenticationService AuthService { get; set; }

        private class Constants
        {
            public static readonly string Password = "11111111";
            public static readonly string DisplayName = "Jean-Guy";
        }

        [Test]
        public async Task Login_AlreadyLoggedIn_ShouldThrowAlreadyLoggedInException()
        {
            // Arrange
            var email = $"{Guid.NewGuid().ToString()}@example.com";
            await FirebaseHelper.CreateUser(email, Constants.Password, Constants.DisplayName);

            // Act & Assert
            await AuthService.LoginWithEmailAndPassword(email, Constants.Password);
            Assert.That(async () => await AuthService.LoginWithEmailAndPassword(email, Constants.Password),
                Throws.Exception.TypeOf<AlreadyLoggedInException>());
        }

        [Test]
        public async Task Login_InvalidCredentials_ShouldThrowAuthenticationException()
        {
            // Arrange
            var email = $"{Guid.NewGuid().ToString()}@example.com";
            await FirebaseHelper.CreateUser(email, Constants.Password, Constants.DisplayName);

            // Act & Assert
            Assert.That(async () => await AuthService.LoginWithEmailAndPassword(email, Constants.Password + "flop"),
                Throws.Exception.TypeOf<AuthenticationException>());
        }

        [Test]
        public async Task Login_ValidCredentials_ShouldEmitCurrentUserChanged()
        {
            // Arrange
            var wasCalled = false;
            var email = $"{Guid.NewGuid().ToString()}@example.com";
            await FirebaseHelper.CreateUser(email, Constants.Password, Constants.DisplayName);
            AuthService.CurrentUserChanged += u => { wasCalled = true; };

            // Act
            var user = await AuthService.LoginWithEmailAndPassword(email, Constants.Password);

            // Assert
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(Constants.DisplayName, user?.DisplayName);
            Assert.AreEqual(email, user?.Email);
        }

        [Test]
        public void Logout_NoUserLoggedIn_ShouldThrowNoUserLoggedInException()
        {
            // Act
            Assert.That(() => AuthService.Logout(), Throws.Exception.TypeOf<NoUserLoggedInException>());
        }

        [Test]
        public async Task Logout_UserLoggedIn_ShouldEmitCurrentUserChangedAndPutCurrentUserToNull()
        {
            // Arrange
            var numberOfCalls = 0;
            var email = $"{Guid.NewGuid().ToString()}@example.com";
            await FirebaseHelper.CreateUser(email, Constants.Password, Constants.DisplayName);
            AuthService.CurrentUserChanged += u => { ++numberOfCalls; };

            // Act
            await AuthService.LoginWithEmailAndPassword(email, Constants.Password);
            AuthService.Logout();

            // Assert
            Assert.AreEqual(2, numberOfCalls);
            Assert.IsNull(AuthService.CurrentUser);
        }
    }
}