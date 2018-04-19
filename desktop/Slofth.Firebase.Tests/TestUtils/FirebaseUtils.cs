using NUnit.Framework;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolyPaint.Tests.Services
{
    class FirebaseHelper
    {
        class Constants
        {
            public static readonly string JsonEndpoint = ".json";
            public static readonly string RulesEndpoint = ".settings/rules.json";
        }

        private HttpClient DatabaseHttpClient { get; set; }

        private string DatabaseSecret => ConfigurationManager.AppSettings.Get("DatabaseSecret");
        private string DatabaseUrl => ConfigurationManager.AppSettings.Get("DatabaseUrl");
        private string ApiKey => ConfigurationManager.AppSettings.Get("ApiKey");
        private string AuthSuffix => $"?auth={DatabaseSecret}";

        public FirebaseHelper()
        {
            DatabaseHttpClient = new HttpClient();
            DatabaseHttpClient.BaseAddress = new Uri(DatabaseUrl);
        }

        public async Task CleanUp()
        {
            await WipeDatabase();
            await ResetRules();
        }

        private async Task WipeDatabase()
        {
            await DatabaseHttpClient.DeleteAsync(Constants.JsonEndpoint + AuthSuffix);
        }

        public async Task UpdateRules(string rulesFilePath)
        {
            var rules = ReadFile("Rules\\" + rulesFilePath);
            await DatabaseHttpClient.PutAsync(Constants.RulesEndpoint + AuthSuffix, new StringContent(rules));
        }

        private async Task ResetRules()
        {
            await UpdateRules("DefaultRules.json");
        }

        private string ReadFile(string filePath)
        {
            return File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData\\" + filePath));
        }

        public async Task ImportDatabase(string filePath)
        {
            var data = ReadFile("Database\\" + filePath);
            await DatabaseHttpClient.PutAsync(Constants.JsonEndpoint + AuthSuffix, new StringContent(data));
        }
    }
}
