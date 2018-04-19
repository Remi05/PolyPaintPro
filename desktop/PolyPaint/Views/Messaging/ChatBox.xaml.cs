using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Messaging;

namespace PolyPaint.Views.Messaging
{
    public partial class ChatBox : UserControl
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
                    viewModel.OnMessageReceived += MessagesScrollViewer.ScrollToEnd;
                }   
            }
        }

        public ChatBox()
        {
            InitializeComponent();
            MessagesScrollViewer.ScrollToEnd();
        }
    }
}