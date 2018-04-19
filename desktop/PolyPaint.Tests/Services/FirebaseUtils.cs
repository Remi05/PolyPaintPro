using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using NUnit.Framework;
using PolyPaint.Services.Auth;

namespace PolyPaint.Tests.Services
{
    internal class FirebaseHelper
    {
        public FirebaseHelper()
        {
            CreatedUsers = new List<FirebaseAuthLink>();
            Http = new HttpClient();

            AuthProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var options = new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(DatabaseSecret) };
            Database = new FirebaseClient(DatabaseUrl, options);
        }

        private static List<FirebaseAuthLink> CreatedUsers { get; set; }
        public FirebaseAuthProvider AuthProvider { get; }
        public FirebaseClient Database { get; }

        private HttpClient Http { get; }

        private string DatabaseSecret => ConfigurationManager.AppSettings.Get("DatabaseSecret");
        private string DatabaseUrl => ConfigurationManager.AppSettings.Get("DatabaseUrl");
        private string ApiKey => ConfigurationManager.AppSettings.Get("ApiKey");

        public async Task<FirebaseAuthLink> CreateUser(string email, string password, string displayName = "")
        {
            var user = await AuthProvider.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
            CreatedUsers.Add(user);
            return user;
        }

        public async Task<FirebaseAuthLink> CreateAndLogInRandomUser(IAuthenticationService authService)
        {
            var email = $"{Guid.NewGuid().ToString()}@example.com";
            var password = Guid.NewGuid().ToString();
            var displayName = Guid.NewGuid().ToString();

            var user = await CreateUser(email, password, displayName);

            await authService.LoginWithEmailAndPassword(email, password);

            return user;
        }

        public async Task CleanUp()
        {
            await WipeUsers();
            await WipeDatabase();
            await ResetRules();
        }

        private async Task WipeUsers()
        {
            foreach (var user in CreatedUsers)
            {
                await AuthProvider.DeleteUser(user.FirebaseToken);
            }

            CreatedUsers.Clear();
        }

        private async Task WipeDatabase()
        {
            await Http.DeleteAsync($"https://polypaintprotests.firebaseio.com/.json?auth={DatabaseSecret}");
        }

        public async Task UpdateRules(string rulesFileName)
        {
            var rules = ReadRulesFile(rulesFileName);
            await Http.PutAsync($"https://polypaintprotests.firebaseio.com/.settings/rules.json?auth={DatabaseSecret}", new StringContent(rules));
        }

        private async Task ResetRules()
        {
            await UpdateRules("DefaultRules.json");
        }

        private string ReadRulesFile(string fileName)
        {
            return File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData\\Rules\\" + fileName));
        }
    }
}