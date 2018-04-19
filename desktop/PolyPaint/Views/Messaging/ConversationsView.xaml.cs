using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Messaging;

namespace PolyPaint.Views.Messaging
{
    public partial class ConversationsView : UserControl
    {
        [Dependency]
        public IConversationsViewModel ViewModel { set { DataContext = value; } }

        public ConversationsView()
        {
            InitializeComponent();
        }
    }
}
