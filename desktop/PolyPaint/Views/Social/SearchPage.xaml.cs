using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Social;

namespace PolyPaint.Views.Social
{
    public partial class SearchPage : UserControl
    {
        private ISearchPageViewModel viewModel;
        [Dependency]
        public ISearchPageViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public SearchPage()
        {
            InitializeComponent();
        }
    }
}
