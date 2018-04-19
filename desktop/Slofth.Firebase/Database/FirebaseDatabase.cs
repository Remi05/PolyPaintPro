using Slofth.Firebase.Http;
using Slofth.Firebase.Utils;
using System;
using System.Threading.Tasks;

namespace Slofth.Firebase.Database
{
    public class FirebaseDatabase
    {
        private Func<Task<string>> TokenIdFactory { get; set; }

        private IFirebaseHttpClientFacade Client { get; set; }
        private string DatabaseUrl { get; set; }

        public FirebaseDatabase(string databaseUrl, Func<Task<string>> tokenIdFactory)
        {
            DatabaseUrl = databaseUrl;
            TokenIdFactory = tokenIdFactory;

            Client = FirebaseHttpClientFactory.CreateFirebaseDatabaseHttpClient();
        }

        public ChildQuery Ref(string name)
        {
            var builder = UrlBuilder
                .Create(DatabaseUrl)
                .AppendToPath(name);

            return new ChildQuery(builder, name, TokenIdFactory);
        }
    }
}
