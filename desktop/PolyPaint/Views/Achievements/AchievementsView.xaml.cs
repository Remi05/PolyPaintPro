using System.Windows.Controls;
using Unity.Attributes;
using PolyPaint.ViewModels.Achievements;

namespace PolyPaint.Views.Achievements
{
    public partial class AchievementsView : UserControl
    {
        private IAchievementsViewModel viewModel;
        [Dependency]
        public IAchievementsViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public AchievementsView()
        {
            InitializeComponent();
        }
    }
}