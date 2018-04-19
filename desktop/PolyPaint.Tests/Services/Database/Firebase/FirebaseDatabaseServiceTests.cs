using System.Threading.Tasks;
using NUnit.Framework;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Database;

namespace PolyPaint.Tests.Services.Database.Firebase
{
    [TestFixture]
    internal class FirebaseDatabaseServiceTests
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
        private IDatabaseService Database { get; set; }

        private class Constants
        {
            public static readonly string Password = "11111111";
            public static readonly string DisplayName = "Jean-Guy";
        }

        [Test]
        public async Task ChildAndPush_KeyIsSet()
        {
            // Act
            var childA = Database.Ref("A");
            var childB = childA.Child("B");
            var childC = childA.Child("C");
            var push = await childA.Push();

            // Assert
            Assert.AreEqual(childA.Key, "A");
            Assert.AreEqual(childB.Key, "B");
            Assert.AreEqual(childC.Key, "C");
            Assert.NotNull(push);
        }

        [Test]
        public async Task Read_UserAllowed_ShouldRetreiveData()
        {
            // Arrange
            var expectedPerson = new Person("John Cena", 32, "You can't see it");
            var firebaseObject = await FirebaseHelper.Database.Child("my_data").PostAsync(expectedPerson.ToJson());
            await FirebaseHelper.UpdateRules("NoRules.json");

            var user = await FirebaseHelper.CreateAndLogInRandomUser(AuthService);
            Database = new FirebaseDatabaseService(AuthService);

            // Act
            var actualPerson = await Database.Ref("my_data").Child(firebaseObject.Key).Once<Person>();

            // Assert
            Assert.AreEqual(expectedPerson.ToJson(), actualPerson.ToJson());
        }

        [Test]
        [Ignore("Exceptions aren't wrapped yet.")]
        public async Task Read_UserNotAllowed_ShouldBeDenied()
        {
            // Arrange
            var expectedPerson = new Person("John Cena", 32, "You can't see it");
            var firebaseObject = await FirebaseHelper.Database.Child("my_data").PostAsync(expectedPerson.ToJson());
            await FirebaseHelper.UpdateRules("DenyEverything.json");

            var user = await FirebaseHelper.CreateAndLogInRandomUser(AuthService);
            Database = new FirebaseDatabaseService(AuthService);

            // Act
            Assert.That(async () => await Database.Ref("my_data").Child(firebaseObject.Key).Once<Person>(),
                Throws.Exception.TypeOf<PermissionDeniedException>());
        }

        [Test]
        [Ignore("Exceptions aren't wrapped yet.")]
        public async Task Update_UserNotAllowed_ShouldBeDenied()
        {
            // Arrange
            var expectedPerson = new Person("John Cena", 32, "You can't see it");
            var firebaseObject = await FirebaseHelper.Database.Child("my_data").PostAsync(expectedPerson.ToJson());
            await FirebaseHelper.UpdateRules("DenyEverything.json");

            var user = await FirebaseHelper.CreateAndLogInRandomUser(AuthService);
            Database = new FirebaseDatabaseService(AuthService);

            // Act & Assert
            Assert.That(async () => await Database
                    .Ref("my_data")
                    .Child(firebaseObject.Key)
                    .Set(new Person("Johnny", 2, "")),
                Throws.Exception.TypeOf<PermissionDeniedException>());
        }


        [Test]
        [Ignore("Exceptions aren't wrapped yet.")]
        public async Task Write_UserNotAllowed_ShouldBeDenied()
        {
            // Arrange
            await FirebaseHelper.UpdateRules("DenyEverything.json");
            var user = await FirebaseHelper.CreateAndLogInRandomUser(AuthService);
            Database = new FirebaseDatabaseService(AuthService);

            // Act & Assert
            Assert.That(async () => await Database.Ref("my_data").Set("3"),
                Throws.Exception.TypeOf<PermissionDeniedException>());
        }
    }
}