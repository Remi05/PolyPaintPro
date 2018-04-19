using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Social;

namespace PolyPaint.Views.Social
{
    public partial class SearchDrawingView : UserControl
    {
        private ISearchDrawingViewModel viewModel;
        [Dependency]
        public ISearchDrawingViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public SearchDrawingView()
        {
            InitializeComponent();
        }
    }
}
