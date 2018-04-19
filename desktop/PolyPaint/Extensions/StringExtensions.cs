using PolyPaint.Utils;
using System;
using System.Text;

namespace PolyPaint.Extensions
{
    public static class StringExtensions
    {
        // Ref.: https://bit.ly/2EGiEFn
        public static string Base64Encode(this string str)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(plainTextBytes);
        }

        // Ref.: https://bit.ly/2EGiEFn
        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string Base32Encode(this string str)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(str);
            return Base32.ToBase32String(plainTextBytes);
        }

        public static string Base32Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Base32.FromBase32String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
