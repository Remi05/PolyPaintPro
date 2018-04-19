using PolyPaint.ViewModels.Social;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Unity.Attributes;
using Windows.UI.Xaml.Shapes;

namespace PolyPaint.Views.Social
{
    public partial class Story : UserControl
    {
        private IStoryViewModel ViewModel => DataContext as IStoryViewModel;
        private Storyboard Storyboard { get; set; }

        public Story()
        {
            InitializeComponent();
            Storyboard = new Storyboard();

            DataContextChanged += (s, e) =>
            {
                if (e.OldValue != null)
                {
                    ((IStoryViewModel)e.OldValue).PropertyChanged -= ViewModel_PropertyChanged;
                }

                if (e.NewValue != null)
                {
                    ((IStoryViewModel)e.NewValue).PropertyChanged += ViewModel_PropertyChanged;
                }
            };
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.Story))
            {
                BindProgressBarAnimation();
            }
        }

        private void BindProgressBarAnimation()
        {
            Storyboard.Children.Clear();
            var fade = new DoubleAnimation()
            {
                From = 0,
                To = MainStackPanel.Width,
                Duration = ViewModel.StoryDuration,
            };

            Storyboard.SetTarget(fade, ProgressBar);
            Storyboard.SetTargetProperty(fade, new PropertyPath(WidthProperty));

            Storyboard.Children.Add(fade);

            fade.Completed += (_, __) => ViewModel?.Close?.Execute(null);

            Storyboard.Begin();
        }
    }
}
