using System;

namespace Slofth.Firebase
{
    public class FirebaseException : Exception
    {
        public FirebaseException(string message = null) : base(message) { }
    }

    public class InvalidRefreshTokenException : Exception {
        public InvalidRefreshTokenException(string message = null) : base(message) { }
    }

    public class InvalidGrantTypeException : Exception {
        public InvalidGrantTypeException(string message = null) : base(message) { }
    }

    public class MissingRefreshTokenException : Exception {
        public MissingRefreshTokenException(string message = null) : base(message) { }
    }

    public class EmailExistsException : Exception {
        public EmailExistsException(string message = null) : base(message) { }
    }

    public class OperationNotAllowedException : Exception {
        public OperationNotAllowedException(string message = null) : base(message) { }
    }

    public class TooManyAttemptsException : Exception {
        public TooManyAttemptsException(string message = null) : base(message) { }
    }

    public class EmailNotFoundException : Exception {
        public EmailNotFoundException(string message = null) : base(message) { }
    }

    public class InvalidPasswordException : Exception {
        public InvalidPasswordException(string message = null) : base(message) { }
    }

    public class UserDisabledException : Exception {
        public UserDisabledException(string message = null) : base(message) { }
    }

    public class InvalidIdpResponseException : Exception {
        public InvalidIdpResponseException(string message = null) : base(message) { }
    }

    public class ExpiredOobCodeException : Exception {
        public ExpiredOobCodeException(string message = null) : base(message) { }
    }

    public class InvalidOobCodeException : Exception {
        public InvalidOobCodeException(string message = null) : base(message) { }
    }

    public class InvalidIdTokenException : Exception {
        public InvalidIdTokenException(string message = null) : base(message) { }
    }

    public class WeakPasswordException : Exception {
        public WeakPasswordException(string message = null) : base(message) { }
    }

    public class UserNotFoundException : Exception {
        public UserNotFoundException(string message = null) : base(message) { }
    }

    public class CredentialTooOldException : Exception {
        public CredentialTooOldException(string message = null) : base(message) { }
    }

    public class TokenExpiredException : Exception {
        public TokenExpiredException(string message = null) : base(message) { }
    }

    public class FederateUserIdAlreadyLinkedException : Exception {
        public FederateUserIdAlreadyLinkedException(string message = null) : base(message) { }
    }
}