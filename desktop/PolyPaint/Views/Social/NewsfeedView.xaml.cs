using PolyPaint.ViewModels.Social;
using System.Windows.Controls;
using Unity.Attributes;

namespace PolyPaint.Views.Social
{
    public partial class NewsfeedView : UserControl
    {
        private INewsfeedViewModel viewModel;
        [Dependency]
        public INewsfeedViewModel ViewModel
        {
            get => viewModel;
            set
            {
                DataContext = viewModel = value;
            }
        }

        public NewsfeedView()
        {
            InitializeComponent();
        }
    }
}
