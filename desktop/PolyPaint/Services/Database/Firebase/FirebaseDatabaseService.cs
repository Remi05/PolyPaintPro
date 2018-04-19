using System.Configuration;
using System.Threading.Tasks;
using PolyPaint.Services.Auth;
using Slofth.Firebase.Database;

namespace PolyPaint.Services.Database
{
    public class FirebaseDatabaseService : IDatabaseService
    {
        private string DatabaseUrl => ConfigurationManager.AppSettings.Get("DatabaseUrl");
        private FirebaseDatabase BaseDatabase { get; }

        public FirebaseDatabaseService(IAuthenticationService authService)
        {
            BaseDatabase = new FirebaseDatabase(DatabaseUrl, () => Task.FromResult(authService.DatabaseToken));
        }

        public IChildQuery Ref(string name)
        {
            return new FirebaseChildQuery(BaseDatabase.Ref(name));
        }
    }
}