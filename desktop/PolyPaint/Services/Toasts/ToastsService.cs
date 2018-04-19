using PolyPaint.Views.Toasts;
using System.Collections.Generic;
using System.Windows;

namespace PolyPaint.Services.Toasts
{
    class ToastsService : IToastsService
    {
        private List<Toast> Toasts { get; set; } = new List<Toast>();

        public void Pop(string title, string description, string imageUri)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var toast = new Toast()
                {
                    Title = title,
                    Description = description,
                    ImageUri = imageUri,
                    Width = Constants.ToastWidth,
                    Height = Constants.ToastHeight,
                    Duration = Constants.Duration,
                    Owner = App.Current.MainWindow
                };

                Toasts.Add(toast);
                Realign(toast);

                toast.Closed += (s, e) =>
                {
                    Toasts.Remove(toast);
                    App.Current.MainWindow.LocationChanged -= (_, __) => Realign(toast);
                    App.Current.MainWindow.StateChanged -= (_, __) => Realign(toast);
                    App.Current.MainWindow.SizeChanged -= (_, __) => Realign(toast);
                };

                App.Current.MainWindow.LocationChanged += (_, __) => Realign(toast);
                App.Current.MainWindow.StateChanged += (_, __) => Realign(toast);
                App.Current.MainWindow.SizeChanged += (_, __) => Realign(toast);

                toast.Show();
            });
        }

        private void Realign(Toast toast)
        {
            var position = ComputeNextPosition(Toasts.IndexOf(toast));
            toast.Top = position.Y;
            toast.Left = position.X;
        }

        private Point ComputeNextPosition(int index)
        {
            Point position;
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                var workArea = SystemParameters.WorkArea;
                position = new Point(
                  workArea.Right - Constants.ToastWidth - Constants.XMargin,
                  workArea.Bottom - (Constants.ToastHeight + Constants.YMargin) * (index + 1)
              );
            }
            else
            {
                position = new Point(
                    (App.Current.MainWindow.Left + App.Current.MainWindow.Width) - Constants.ToastWidth - Constants.XMargin,
                    (App.Current.MainWindow.Top + App.Current.MainWindow.Height) - (Constants.ToastHeight + Constants.YMargin) * (index + 1)
                );
            }

            return position;
        }

        private static class Constants
        {
            public static readonly int ToastWidth = 400;
            public static readonly int ToastHeight = 110;
            public static readonly int YMargin = 10;
            public static readonly int XMargin = 30;
            public static readonly int Duration = 5000;
        }
    }
}
