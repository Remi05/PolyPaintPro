using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PolyPaint.Converters
{
    internal class WidthToColumnsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double gridWidth = (double)value;
            double thumbnailWidth = (double)parameter;
            return (int)gridWidth / (int)thumbnailWidth;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class BoolToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "/Resources/NSFW-stamp-BW.png";
            return (bool)value ? "/Resources/NSFW-stamp.png" : "/Resources/NSFW-stamp-BW.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class BoolToRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;
            return (bool)value ? 20 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
