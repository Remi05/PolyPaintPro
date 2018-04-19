using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Auth;
using System.Windows.Input;

namespace PolyPaint.Views.Auth
{
    public partial class LoginView : UserControl
    {
        private ILoginViewModel viewModel;
        [Dependency]
        public ILoginViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public LoginView()
        {
            InitializeComponent();
        }
    }
}