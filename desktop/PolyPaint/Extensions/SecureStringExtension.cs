using System;
using System.Runtime.InteropServices;
using System.Security;

namespace PolyPaint.Extensions
{
    public static class SecureStringExtension
    {
        // Ref.: http://bit.ly/2mOmQMx
        public static string ToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null) return null;

            var unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}