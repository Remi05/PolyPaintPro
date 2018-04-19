using PolyPaint.Models;
using PolyPaint.ViewModels.Social;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity.Attributes;

namespace PolyPaint.Views.Social
{
    public partial class StoriesBar : UserControl
    {
        private IStoriesBarViewModel viewModel;
        [Dependency]
        public IStoriesBarViewModel ViewModel
        {
            get => viewModel;
            set { DataContext = viewModel = value; }
        }

        public static DependencyProperty ShowStoryCommandProperty = DependencyProperty.Register("ShowStoryCommand", typeof(ICommand), typeof(StoriesBar));

        public ICommand ShowStoryCommand
        {
            get => (ICommand)GetValue(ShowStoryCommandProperty);
            set => SetValue(ShowStoryCommandProperty, value);
        }

        public StoriesBar()
        {
            InitializeComponent();
        }

        private void StoryPreview_Click(DetailedStoryModel story)
        {
            if (!ShowStoryCommand?.CanExecute(null) ?? true)
                return;

            ShowStoryCommand?.Execute(story);
        }
    }
}
