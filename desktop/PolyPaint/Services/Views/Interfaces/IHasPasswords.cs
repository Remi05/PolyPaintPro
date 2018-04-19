using System.Security;

namespace PolyPaint.ViewInterfaces
{
    public interface IHasPasswords
    {
        SecureString[] Passwords { get; }
    }
}