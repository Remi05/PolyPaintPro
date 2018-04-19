using System;
using System.Net.Mail;

namespace PolyPaint.Utils
{
    public static class EmailHelper
    {
        public static bool IsValid(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
