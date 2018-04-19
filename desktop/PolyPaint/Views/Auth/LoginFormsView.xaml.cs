using System.Security;
using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewInterfaces;
using PolyPaint.ViewModels.Auth;

namespace PolyPaint.Views.Auth
{
    public partial class LoginFormsView : UserControl, IHasPassword
    {
        private ILoginFormsViewModel viewModel;
        [Dependency]
        public ILoginFormsViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public SecureString Password => PasswordField.SecurePassword;

        public LoginFormsView()
        {
            InitializeComponent();
        }
    }
}