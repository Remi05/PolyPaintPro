using PolyPaint.ViewModels;
using System.Windows.Controls;
using Unity.Attributes;

namespace PolyPaint.Views
{
    public partial class TutorialView : UserControl
    {
        private ITutorialViewModel viewModel;
        [Dependency]
        public ITutorialViewModel ViewModel
        {
            get => viewModel;
            set => DataContext = viewModel = value;
        }

        public TutorialView()
        {
            InitializeComponent();
        }
    }
}
