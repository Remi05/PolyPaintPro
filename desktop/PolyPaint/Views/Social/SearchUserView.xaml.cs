using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Social;

namespace PolyPaint.Views.Social
{
    public partial class SearchUserView : UserControl
    {
        private ISearchUserViewModel viewModel;
        [Dependency]
        public ISearchUserViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public SearchUserView()
        {
            InitializeComponent();
        }
    }
}
