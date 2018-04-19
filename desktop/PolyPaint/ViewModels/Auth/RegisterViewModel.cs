using System;
using Firebase.Auth;
using PolyPaint.Extensions;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Logger;
using PolyPaint.Utils;
using PolyPaint.ViewInterfaces;

namespace PolyPaint.ViewModels.Auth
{
    public interface IRegisterViewModel : IViewModel
    {
        event Action ConnectClicked;
        event Action AccountSuccessfullyCreated;

        string DisplayName { get; }
        string Email { get; }
        string ErrorMessage { get; }
        bool IsLoading { get; }

        RelayCommand<IHasPasswords> RegisterCommand { get; }
        RelayCommand<object> SwitchToLoginViewCommand { get; }
    }

    public class RegisterViewModel : ViewModel, IRegisterViewModel
    {
        public event Action ConnectClicked;
        public event Action AccountSuccessfullyCreated;

        public ILogger Logger { get; }
        private IAuthenticationService AuthService { get; }

        private string email;
        public string Email
        {
            get => email;
            set
            {
                if (String.Equals(email, value))
                    return;

                email = value;
                RaisePropertyChanged();
            }
        }

        private string displayName;
        public string DisplayName
        {
            get => displayName;
            set
            {
                if (!string.Equals(displayName, value))
                {
                    displayName = value;
                    RaisePropertyChanged();
                }
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
            set
            {
                isLoading = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand<IHasPasswords> RegisterCommand { get; }
        public RelayCommand<object> SwitchToLoginViewCommand { get; }

        public RegisterViewModel(IAuthenticationService authService, ILogger logger)
        {
            AuthService = authService;
            Logger = logger;
            RegisterCommand = new RelayCommand<IHasPasswords>(Register);
            SwitchToLoginViewCommand = new RelayCommand<object>((_) => SwitchToLoginView());
        }

        private async void Register(IHasPasswords securedPasswords)
        {
            if (!AreFieldsFilled(securedPasswords))
            {
                ErrorMessage = "You must fill all the fields to login.";
                return;
            }

            if (!EmailHelper.IsValid(Email))
            {
                ErrorMessage = "The provided email address is not in a valid format.";
                return;
            }

            if (!IsPasswordValid(securedPasswords))
            {
                ErrorMessage = "The password must contain at least 6 characters.";
                return;
            }

            if (!ArePasswordsEqual(securedPasswords))
            {
                ErrorMessage = "The entered passwords are not identical.";
                return;
            }

            ErrorMessage = "";
            try
            {
                IsLoading = true;
                await AuthService.CreateUserWithEmailAndPassword(Email, securedPasswords.Passwords[0].ToUnsecureString(), DisplayName);
                AccountSuccessfullyCreated.Invoke();
            }
            catch (FirebaseAuthException ex)
            {
                Logger.Error("An error occured while creating the account.", ex);
                if (ex.Reason.ToString() == "EmailExists")
                {
                    ErrorMessage = "An account already exists with this email address.";
                }
                else if (ex.Message == "INVALID_EMAIL")
                {
                    ErrorMessage = "The provided email address is not in a valid format.";
                }
                else
                {
                    ErrorMessage = "An error occured while creating the account.";
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured while creating the account.", ex);
                ErrorMessage = "An unidentified error occured while creating the account.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SwitchToLoginView()
        {
            ConnectClicked.Invoke();
        }

        private bool AreFieldsFilled(IHasPasswords securedPasswords)
        {
            return !String.IsNullOrWhiteSpace(DisplayName)
                && !String.IsNullOrWhiteSpace(Email)
                && securedPasswords?.Passwords[0] != null
                && securedPasswords?.Passwords[1] != null;
        }

        private bool ArePasswordsEqual(IHasPasswords securedPasswords)
        {
            var passwordA = securedPasswords.Passwords[0].ToUnsecureString();
            var passwordB = securedPasswords.Passwords[1].ToUnsecureString();
            return passwordA == passwordB;
        }

        private bool IsPasswordValid(IHasPasswords securedPasswords)
        {
            return securedPasswords?.Passwords[0]?.Length >= 6;
        }
    }
}