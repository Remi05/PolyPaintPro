using System.Security;

namespace PolyPaint.ViewInterfaces
{
    public interface IHasPassword
    {
        SecureString Password { get; }
    }
}