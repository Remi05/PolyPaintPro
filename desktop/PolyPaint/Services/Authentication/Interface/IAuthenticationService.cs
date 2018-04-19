using PolyPaint.Models;
using System.Threading.Tasks;

namespace PolyPaint.Services.Auth
{
    public delegate void CurrentUserChangedHandler(User user);

    public interface IAuthenticationService
    {
        string OfflineClientId { get; }

        event CurrentUserChangedHandler BeforeLogout;
        event CurrentUserChangedHandler CurrentUserChanged;
        User CurrentUser { get; }
        string DatabaseToken { get; }
        string GoogleAccessToken { get; set; }
        string FacebookAccessToken { get; set; }
        bool IsLoggedIn { get; }
        bool IsLoggedInWithFacebook { get; }

        Task<User> LoginWithEmailAndPassword(string email, string password);
        Task<User> LoginWithFacebook(string facebookAccessToken);
        Task<User> LoginWithGoogle(string googleAccessToken);
        Task<User> CreateUserWithEmailAndPassword(string email, string password, string displayName = "", bool sendVerificationEmail = false);
        void Logout();
    }
}