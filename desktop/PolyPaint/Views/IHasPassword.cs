using System.Security;

namespace PolyPaint.Views
{
    public interface IHasPassword
    {
        SecureString Password { get; }
    }
}