using PolyPaint.Extensions;
using PolyPaint.Services;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Drawing;
using PolyPaint.Services.Logger;
using PolyPaint.Services.Social;
using PolyPaint.Utils;
using PolyPaint.ViewInterfaces;
using PolyPaint.Views;
using System.Net.Mail;
using System.Net;
using System;
using System.Configuration;
using PolyPaint.Services.Email;
using Firebase.Auth;

namespace PolyPaint.ViewModels.Auth
{
    public interface ILoginFormsViewModel : IViewModel
    {
        event Action CreateAccountClicked;
        event Action LoginSucceeded;

        string Username { get; set; }
        string ErrorMessage { get; }
        bool IsLoading { get; }

        RelayCommand<IHasPassword> LoginCommand { get; }
        RelayCommand<object> LoginWithFacebookCommand { get; }
        RelayCommand<object> LoginWithGoogleCommand { get; }
        RelayCommand<object> SwitchToRegisterViewCommand { get; }
    }

    public class LoginFormsViewModel : ViewModel, ILoginFormsViewModel
    {
        public event Action CreateAccountClicked;
        public event Action LoginSucceeded;

        private IAuthenticationService AuthService { get; set; }
        private ILogger Logger { get; set; }
        private BrowserView BrowserWindow { get; set; }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                if (string.Equals(username, value))
                    return;

                username = value;
                RaisePropertyChanged();
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            private set { errorMessage = value; RaisePropertyChanged(); }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            private set { isLoading = value; RaisePropertyChanged(); }
        }

        public RelayCommand<IHasPassword> LoginCommand { get; }
        public RelayCommand<object> LoginWithFacebookCommand { get; }
        public RelayCommand<object> LoginWithGoogleCommand { get; }
        public RelayCommand<object> SwitchToRegisterViewCommand { get; }

        public LoginFormsViewModel(IAuthenticationService authService, ILogger logger)
        {
            AuthService = authService;
            Logger = logger;
            LoginCommand = new RelayCommand<IHasPassword>(Login);
            LoginWithFacebookCommand = new RelayCommand<object>((_) => LoginWithFacebook());
            LoginWithGoogleCommand = new RelayCommand<object>((_) => LoginWithGoogle());
            SwitchToRegisterViewCommand = new RelayCommand<object>((_) => SwitchToRegisterView());
        }

        private async void Login(IHasPassword securedPassword)
        {
            if (securedPassword == null || string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Please enter your email and password to login.";
                return;
            }

            IsLoading = true;

            try
            {
                await AuthService.LoginWithEmailAndPassword(Username, securedPassword.Password.ToUnsecureString());
                LoginSucceeded?.Invoke();
            }
            catch (AuthenticationException ex)
            {
                Logger.Error("An error occured while logging in.", ex);

                if ((ex.InnerException as dynamic).Reason == AuthErrorReason.WrongPassword
                 || (ex.InnerException as dynamic).Reason == AuthErrorReason.UnknownEmailAddress)
                {
                    ErrorMessage = "Invalid username/password combination.";
                }
                else
                {
                    ErrorMessage = "An error occured while logging in.";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoginWithFacebook()
        {
            IsLoading = true;
            if (BrowserWindow != null)
                return;

            BrowserWindow = new BrowserView(FacebookAPI.FacebookAuthenticationUri);

            BrowserWindow.FacebookConnected += Browser_FacebookConnected;
            BrowserWindow.Closed += Browser_Closed;
            BrowserWindow.Show();
        }

        private void LoginWithGoogle()
        {
            IsLoading = true;
            if (BrowserWindow != null)
                return;

            BrowserWindow = new BrowserView(GoogleAPI.GoogleAuthenticationUri);
            BrowserWindow.GoogleConnected += Browser_GoogleConnected;
            BrowserWindow.Closed += Browser_Closed;
            BrowserWindow.Show();
        }

        private void SwitchToRegisterView()
        {
            CreateAccountClicked?.Invoke();
        }

        private void Browser_Closed(object sender, EventArgs e)
        {
            IsLoading = false;
            BrowserWindow = null;
        }

        private async void Browser_FacebookConnected(object sender, EventArgs e)
        {
            var args = e as ConnectedEventArgs;
            try
            {
                await AuthService.LoginWithFacebook(args.ConnectionToken);
                LoginSucceeded?.Invoke();
            }
            catch (AuthenticationException ex)
            {
                Logger.Error("An error occured while authenticating with Facebook", ex);
                ErrorMessage = "An error occured while authenticating with Facebook.";
            }
            finally
            {
                BrowserWindow?.Close();
            }
        }

        private async void Browser_GoogleConnected(object sender, EventArgs e)
        {
            var args = e as ConnectedEventArgs;
            try
            {
                BrowserWindow.Visibility = System.Windows.Visibility.Hidden;
                await AuthService.LoginWithGoogle(args.ConnectionToken);
                LoginSucceeded?.Invoke();
            }
            catch (AuthenticationException ex)
            {
                Logger.Error("An error occured while authenticating with Google", ex);
                ErrorMessage = "An error occured while authenticating with Google.";
            }
            finally
            {
                BrowserWindow.Close();
            }
        }
    }
}