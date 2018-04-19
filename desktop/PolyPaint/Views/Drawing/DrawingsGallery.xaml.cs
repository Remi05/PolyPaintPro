using PolyPaint.ViewModels.Drawing;
using System.Windows.Controls;
using Unity.Attributes;

namespace PolyPaint.Views.Drawing
{
    public partial class DrawingsGallery : UserControl
    {
        private IDrawingsGalleryViewModel viewModel;
        [Dependency]
        public IDrawingsGalleryViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public DrawingsGallery()
        {
            InitializeComponent();
        }
    }
}
