using PolyPaint.ViewInterfaces;
using PolyPaint.ViewModels.Drawing;
using System.Security;
using System.Windows;
using System.Windows.Input;
using Unity.Attributes;

namespace PolyPaint.Views.Drawing
{

    public partial class DrawingConfigurationWizard : Window, IHasPasswords
    {
        private IDrawingConfigurationWizardViewModel viewModel;
        [Dependency]
        public IDrawingConfigurationWizardViewModel ViewModel
        {
            get => viewModel;
            set => DataContext = viewModel = value;
        }

        public SecureString[] Passwords => new SecureString[] { PasswordField.SecurePassword, ConfirmPasswordField.SecurePassword };

        public DrawingConfigurationWizard()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
