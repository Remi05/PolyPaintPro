using System.Security;
using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewInterfaces;
using PolyPaint.ViewModels.Auth;

namespace PolyPaint.Views.Auth
{
    public partial class RegisterView : UserControl, IHasPasswords
    {
        private IRegisterViewModel viewModel;
        [Dependency]
        public IRegisterViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public SecureString[] Passwords => new SecureString[] { PasswordField.SecurePassword, ConfirmPasswordField.SecurePassword };

        public RegisterView()
        {
            InitializeComponent();
        }
    }
}