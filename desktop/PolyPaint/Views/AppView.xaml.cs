using System.Windows;
using Unity.Attributes;
using PolyPaint.ViewModels;

namespace PolyPaint.Views
{
    public partial class AppView : Window
    {
        [Dependency]
        public AppViewModel ViewModel { set { DataContext = value; } }

        public AppView()
        {
            InitializeComponent();
        }
    }
}
