using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Social;

namespace PolyPaint.Views.Social
{
    public partial class ProfilePage : UserControl
    {
        private IProfilePageViewModel viewModel;
        [Dependency]
        public IProfilePageViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public ProfilePage()
        {
            InitializeComponent();
        }
    }
}
