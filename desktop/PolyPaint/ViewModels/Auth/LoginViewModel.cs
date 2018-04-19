using System.Windows;
using PolyPaint.Services;
using PolyPaint.Services.Logger;
using PolyPaint.Views.Auth;

namespace PolyPaint.ViewModels.Auth
{
    public interface ILoginViewModel : IViewModel
    {
        FrameworkElement ContentControlView { get; set; }
        string SuccessMessage { get; set; }
    }

    public class LoginViewModel : ViewModel, ILoginViewModel
    {
        private ILogger Logger { get; }
        private IViewsManager ViewsManager { get; }

        private string successMessage;
        public string SuccessMessage
        {
            get => successMessage;
            set
            {
                successMessage = value;
                RaisePropertyChanged();
            }
        }

        private FrameworkElement contentControlView;
        public FrameworkElement ContentControlView
        {
            get => contentControlView;
            set { contentControlView = value; RaisePropertyChanged(); }
        }


        public LoginViewModel(IViewsManager viewsManager, ILogger logger)
        {
            ViewsManager = viewsManager;
            Logger = logger;

            SwitchToLoginView();
        }

        private void SwitchToLoginView()
        {
            successMessage = "";
            var view = ViewsManager.GetUserControl<LoginFormsView>();

            view.ViewModel.CreateAccountClicked += SwitchToRegisterView;
            view.ViewModel.LoginSucceeded += () => Logger.Debug("Closing Login view.");

            if (ContentControlView != null)
            {
                view.ViewModel.Username = (ContentControlView as RegisterView)?.ViewModel?.Email;
            }

            ContentControlView = view;            
        }

        private void SwitchToRegisterView()
        {
            var view = ViewsManager.GetUserControl<RegisterView>();

            view.ViewModel.ConnectClicked += SwitchToLoginView;
            view.ViewModel.AccountSuccessfullyCreated += () =>
            {
                successMessage = "Your account was created successfully!";
                SwitchToLoginView();
            };

            ContentControlView = view;
        }
    }
}