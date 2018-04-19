using System;
using System.Windows;
using System.Windows.Input;

namespace PolyPaint.Views
{
    /// <summary>
    /// Interaction logic for FacebookCaptionView.xaml
    /// </summary>
    public partial class FacebookCaptionView : Window
    {
        public event Action<string> CaptionWritten;
        public FacebookCaptionView()
        {
            InitializeComponent();
        }

        public void ShareCaption(object o, RoutedEventArgs args)
        {
            CaptionWritten?.Invoke(Caption.Text);
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
