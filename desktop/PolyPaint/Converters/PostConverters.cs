using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PolyPaint.Converters
{
    internal class NumberOfLikesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{(int)value} like(s)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class LastModifiedOnToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var lastModifiedOn = (DateTime)value;
            var timeDelta = DateTime.Now - lastModifiedOn;
            return timeDelta.Days > 0 ? $"{timeDelta.Days} day(s) ago" :
                   timeDelta.Hours > 0 ? $"{timeDelta.Hours} hour(s) ago" :
                   timeDelta.Minutes > 0 ? $"{timeDelta.Minutes} minute(s) ago" :
                   $"{Math.Max(timeDelta.Seconds, 0)} second(s) ago";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    internal class BoolReportedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "#b71d1d" : "#909396";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)value == "#b71d1d";
        }
    }
}
