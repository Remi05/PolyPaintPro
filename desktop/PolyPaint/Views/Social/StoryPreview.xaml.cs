using PolyPaint.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PolyPaint.Views.Social
{
    public partial class StoryPreview : UserControl
    {
        public event Action<DetailedStoryModel> Click;

        public StoryPreview()
        {
            InitializeComponent();
        }

        private void Picture_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(DataContext as DetailedStoryModel);
        }
    }
}
