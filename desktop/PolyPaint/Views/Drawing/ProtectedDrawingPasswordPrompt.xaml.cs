using PolyPaint.ViewInterfaces;
using PolyPaint.ViewModels.Drawing;
using System.Security;
using System.Windows;
using System.Windows.Input;
using Unity.Attributes;

namespace PolyPaint.Views.Drawing
{

    public partial class ProtectedDrawingPasswordPrompt : Window, IHasPassword
    {
        private IProtectedDrawingPasswordPromptViewModel viewModel;
        [Dependency]
        public IProtectedDrawingPasswordPromptViewModel ViewModel
        {
            get => viewModel;
            set => DataContext = viewModel = value;
        }

        public SecureString Password => PasswordField.SecurePassword;

        public ProtectedDrawingPasswordPrompt()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
