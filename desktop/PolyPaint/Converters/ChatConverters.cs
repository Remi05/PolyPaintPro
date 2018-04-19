using PolyPaint.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PolyPaint.Converters
{
    internal class IsMaximizedToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.MainBackground : ThemeColors.SecondaryBackgroundLight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }     
    }

    internal class IsMaximizedToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ChatColors.IsMaximizedForegroundColor : ChatColors.IsMinimizedForegroundColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class IsMaximizedToHoveredBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.MainBackgroundDark : ThemeColors.SecondaryBackgroundDark;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class IsMaximizedToHoveredForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.MainForeground : ChatColors.IsMinimizedHoveredForegroundColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class IsEnabledToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.MainBackground : ChatColors.IsNotEnabledColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal static class ChatColors
    {
        public static readonly Brush IsMaximizedForegroundColor = (Brush)new BrushConverter().ConvertFrom("#99FFFFFF");
        public static readonly Brush IsMinimizedForegroundColor = (Brush)new BrushConverter().ConvertFrom("#7C7C7C");
        public static readonly Brush IsMinimizedHoveredForegroundColor = (Brush)new BrushConverter().ConvertFrom("#2F2F2F");
        public static readonly Brush IsNotEnabledColor = (Brush)new BrushConverter().ConvertFrom("#AFAFAF");
    }
}