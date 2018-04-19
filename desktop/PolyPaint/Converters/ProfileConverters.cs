using PolyPaint.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PolyPaint.Converters
{
    internal class ImageUrlToSafeProfilePictureUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imageUrl = (string)value;
            return string.IsNullOrWhiteSpace(imageUrl) ? Constants.DefaultImagePath : imageUrl;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static class Constants
        {
            public static readonly string DefaultImagePath = "Resources/anonymous.png";
        }
    }

    internal class FollowingToFollowButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Constants.IsFollowingFollowButtonText : Constants.IsNotFollowingFollowButtonText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static class Constants
        {
            public static readonly string IsFollowingFollowButtonText = "Following ✓";
            public static readonly string IsNotFollowingFollowButtonText = "Follow";
        }
    }

    internal class FollowingToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.SecondaryBackgroundLight : ThemeColors.MainBackground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class FollowingToBackgroundHoveredColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.SecondaryBackgroundDark : ThemeColors.MainBackgroundLight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class FollowingToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? ThemeColors.SecondaryForeground: ThemeColors.MainForeground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
