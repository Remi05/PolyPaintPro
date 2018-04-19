using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace PolyPaint.Views.Toasts
{
    /// <summary>
    /// Interaction logic for Toast.xaml
    /// </summary>
    public partial class Toast : Window
    {
        public string ImageUri { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; } = Constants.ToastDuration; 

        public Toast()
        {
            InitializeComponent();
            DataContext = this;
            Topmost = true;
            Task.Run(() =>
            {
                Thread.Sleep(Constants.ToastDuration);
                App.Current.Dispatcher.Invoke(() => Dismiss());
            });
        }

        private void Dismiss()
        {
            var anim = new DoubleAnimation(0, Constants.FadeOutTime);
            anim.Completed += (_, __) => Close();
            App.Current.Dispatcher.Invoke(() =>
            {
                BeginAnimation(OpacityProperty, anim);
            });
        }

        private static class Constants
        {
            public static readonly TimeSpan FadeOutTime = TimeSpan.FromSeconds(1);
            public static readonly int ToastDuration = 5000;
        }

        private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Dismiss();
        }
    }
}
