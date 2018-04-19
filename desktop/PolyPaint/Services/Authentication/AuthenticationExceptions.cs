using System;

namespace PolyPaint.Services.Auth
{
    public class AuthenticationException : Exception
    {
        public AuthenticationException(Exception innerException)
            : base("An error occured while authenticating", innerException) { }
    }

    public class AlreadyLoggedInException : Exception
    {
        public AlreadyLoggedInException()
            : base("A user is already logged in.") { }
    }

    public class NoUserLoggedInException : Exception
    {
        public NoUserLoggedInException()
            : base("There is currently no user logged in.") { }
    }
}