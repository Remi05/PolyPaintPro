using PolyPaint.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PolyPaint.Converters
{
    internal class IsSenderToBackgroundColorConverter : IValueConverter
    {  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.MainBackground : ThemeColors.SecondaryBackgroundDark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }        
    }

    internal class IsSenderToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.MainForeground : ThemeColors.SecondaryForeground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class IsSenderToAligmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}