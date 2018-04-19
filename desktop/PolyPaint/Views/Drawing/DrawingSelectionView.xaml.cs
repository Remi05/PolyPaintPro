using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Drawing;

namespace PolyPaint.Views.Drawing
{
    public partial class DrawingSelectionView : UserControl
    {
        private IDrawingSelectionViewModel viewModel;
        [Dependency]
        public IDrawingSelectionViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public DrawingSelectionView()
        {
            InitializeComponent();
        }
    }
}
