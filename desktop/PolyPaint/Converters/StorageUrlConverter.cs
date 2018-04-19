using CommonServiceLocator;
using PolyPaint.Services.Auth;
using PolyPaint.Services.Cache;
using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Data;

namespace PolyPaint.Converters
{
    internal class StorageUrlConverter : IValueConverter
    {
        ICacheService Cache { get; set; }
        IAuthenticationService AuthService { get; set; }

        public StorageUrlConverter()
        {
            Cache = ServiceLocator.Current.GetInstance<ICacheService>();
            AuthService = ServiceLocator.Current.GetInstance<IAuthenticationService>();
        }

        private bool IsReachable => NetworkInterface.GetIsNetworkAvailable();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return AuthService.IsLoggedIn && IsReachable
                   ? value
                   : Cache.GetCachedResource(value as string, Constants.UnknownImage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }

        private static class Constants
        {
            public static readonly string UnknownImage = "pack://application:,,,/Resources/nothumbnail.png";
        }
    }
}
