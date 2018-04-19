using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Unity.Attributes;
using PolyPaint.ViewModels.Messaging;

namespace PolyPaint.Views.Messaging
{
    public partial class ConversationsListView : UserControl
    {
        private IConversationsListViewModel viewModel;
        [Dependency]
        public IConversationsListViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public ConversationsListView()
        {
            InitializeComponent();
        }
    }
}
