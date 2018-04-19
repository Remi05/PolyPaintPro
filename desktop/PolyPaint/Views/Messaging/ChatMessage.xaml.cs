using System.Windows.Controls;
using PolyPaint.ViewModels.Messaging;
using Unity.Attributes;

namespace PolyPaint.Views.Messaging
{
    public partial class ChatMessage : UserControl
    {
        private IChatMessageViewModel viewModel;
        [Dependency]
        public IChatMessageViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public ChatMessage()
        {
            InitializeComponent();
        }
    }
}