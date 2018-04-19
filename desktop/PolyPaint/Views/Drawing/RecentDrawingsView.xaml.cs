using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Drawing;

namespace PolyPaint.Views.Drawing
{
    public partial class RecentDrawingsView : UserControl
    {
        private IDrawingsGalleryViewModel viewModel;
        [Dependency]
        public IDrawingsGalleryViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public RecentDrawingsView()
        {
            InitializeComponent();
        }
    }
}
