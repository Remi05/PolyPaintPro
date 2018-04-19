using PolyPaint.ViewModels.Achievements;
using PolyPaint.ViewModels.Social;
using System.Windows.Controls;
using Unity.Attributes;

namespace PolyPaint.Views.Home
{
    public partial class HomeView : UserControl
    {
        [Dependency]
        public IHomeViewModel ViewModel { set { DataContext = value; } }

        public HomeView()
        {
            InitializeComponent();

        }
    }
}
