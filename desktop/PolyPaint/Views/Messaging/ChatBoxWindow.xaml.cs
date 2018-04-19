using System.Windows;
using System.Windows.Input;
using Unity.Attributes;
using PolyPaint.ViewModels.Messaging;

namespace PolyPaint.Views.Messaging
{
    public partial class ChatBoxWindow : Window
    {
        private IChatBoxViewModel viewModel;
        [Dependency]
        public IChatBoxViewModel ViewModel
        {
            get => viewModel;
            set
            {
                DataContext = viewModel = value;
                if (viewModel != null)
                {
                    viewModel.OnClosed += Close;
                }
            }
        }

        public ChatBoxWindow()
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
