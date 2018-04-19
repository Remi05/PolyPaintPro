using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Messaging;

namespace PolyPaint.Views.Messaging
{
    public partial class UserPreview : UserControl
    {
        private IUserPreviewViewModel viewModel;
        [Dependency]
        public IUserPreviewViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public UserPreview()
        {
            InitializeComponent();
        }
    }
}
