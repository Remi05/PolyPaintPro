using Firebase.Auth;
using System.Configuration;
using System.Threading.Tasks;

namespace PolyPaint.Services.Auth
{
    public class FirebaseAuthenticationService : IAuthenticationService
    {
        public event CurrentUserChangedHandler BeforeLogout;
        public event CurrentUserChangedHandler CurrentUserChanged;
        public string GoogleAccessToken { get; set; }
        public string FacebookAccessToken { get; set; }
        public bool IsLoggedIn => CurrentUser != null;
        public bool IsLoggedInWithFacebook => !string.IsNullOrWhiteSpace(FacebookAccessToken);

        public Models.User CurrentUser
        {
            get { return FirebaseUserToUser(CurrentAuthLink?.User); }
        }

        public string DatabaseToken
        {
            get { return CurrentAuthLink?.FirebaseToken; }
        }

        private string ApiKey { get { return ConfigurationManager.AppSettings.Get("ApiKey"); } }
        private FirebaseAuthProvider AuthProvider { get; set; }

        private FirebaseAuthLink currentAuthLink;
        private FirebaseAuthLink CurrentAuthLink
        {
            get => currentAuthLink;
            set {  currentAuthLink = value; CurrentUserChanged?.Invoke(CurrentUser); }
        }

        public string OfflineClientId => "UnknownUser";

        public FirebaseAuthenticationService()
        {
            AuthProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
        }

        public async Task<Models.User> LoginWithEmailAndPassword(string email, string password)
        {
            if (IsLoggedIn)
                return CurrentUser;

            try
            {
                CurrentAuthLink = await AuthProvider.SignInWithEmailAndPasswordAsync(email, password);
                return CurrentUser;
            }
            catch (FirebaseAuthException innerException)
            {
                throw new AuthenticationException(innerException);
            }
        }

        public async Task<Models.User> LoginWithFacebook(string facebookAccessToken)
        {
            FacebookAccessToken = facebookAccessToken;
            try
            {
                CurrentAuthLink = await AuthProvider.SignInWithOAuthAsync(FirebaseAuthType.Facebook, facebookAccessToken);
                return CurrentUser;
            }
            catch (FirebaseAuthException innerException)
            {
                throw new AuthenticationException(innerException);
            }
        }

        public async Task<Models.User> LoginWithGoogle(string googleAccessToken)
        {
            GoogleAccessToken = googleAccessToken;
            try
            {
                CurrentAuthLink = await AuthProvider.SignInWithOAuthAsync(FirebaseAuthType.Google, googleAccessToken);
                return CurrentUser;
            }
            catch (FirebaseAuthException innerException)
            {
                throw new AuthenticationException(innerException);
            }
        }

        public void Logout()
        {
            if (!IsLoggedIn) { throw new NoUserLoggedInException(); }
            BeforeLogout?.Invoke(CurrentUser);
            FacebookAccessToken = null;
            GoogleAccessToken = null;
            CurrentAuthLink = null;
        }

        public async Task<Models.User> CreateUserWithEmailAndPassword(string email, string password, string displayName = "", bool sendVerificationEmail = false)
        {
            var authLink = await AuthProvider.CreateUserWithEmailAndPasswordAsync(email, password, displayName, sendVerificationEmail);
            return FirebaseUserToUser(authLink?.User);
        }

        private Models.User FirebaseUserToUser(User user)
        {
            if (user == null)
                return null;
            return new Models.User(user.LocalId, user.DisplayName, user.Email, user.PhotoUrl);
        }
    }
}