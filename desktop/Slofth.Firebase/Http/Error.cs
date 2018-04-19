using System;

namespace Slofth.Firebase.Http
{
    public class Error
    {
        private static class Messages
        {
            public static readonly string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
            public static readonly string InvalidGrantType = "INVALID_GRANT_TYPE";
            public static readonly string MissingRefreshToken = "MISSING_REFRESH_TOKEN";
            public static readonly string EmailExists = "EMAIL_EXISTS";
            public static readonly string OperationNotAllowed = "OPERATION_NOT_ALLOWED";
            public static readonly string TooManyAttempts = "TOO_MANY_ATTEMPTS_TRY_LATER";
            public static readonly string EmailNotFound = "EMAIL_NOT_FOUND";
            public static readonly string InvalidPassword = "INVALID_PASSWORD";
            public static readonly string UserDisabled = "USER_DISABLED";
            public static readonly string InvalidIdpResponse = "INVALID_IDP_RESPONSE";
            public static readonly string ExpiredOobCode = "EXPIRED_OOB_CODE";
            public static readonly string InvalidOobCode = "INVALID_OOB_CODE";
            public static readonly string InvalidIdToken = "INVALID_ID_TOKEN:The ";
            public static readonly string WeakPassword = "WEAK_PASSWORD";
            public static readonly string UserNotFound = "USER_NOT_FOUND";
            public static readonly string CredentialTooOld = "CREDENTIAL_TOO_OLD_LOGIN_AGAIN";
            public static readonly string TokenExpired = "TOKEN_EXPIRED";
            public static readonly string FederateUserIdAlreadyLinked = "FEDERATED_USER_ID_ALREADY_LINKED";
        }

        public string code { get; set; }
        public string message { get; set; }

        public Exception GetCorrespondingException()
        {
            if (message == Messages.InvalidRefreshToken)
                return new InvalidRefreshTokenException();

            if (message == Messages.InvalidGrantType)
                return new InvalidGrantTypeException();

            if (message == Messages.MissingRefreshToken)
                return new MissingRefreshTokenException();

            if (message == Messages.EmailExists)
                return new EmailExistsException();

            if (message == Messages.OperationNotAllowed)
                return new OperationNotAllowedException();

            if (message == Messages.TooManyAttempts)
                return new TooManyAttemptsException();

            if (message == Messages.EmailNotFound)
                return new EmailNotFoundException();

            if (message == Messages.InvalidPassword)
                return new InvalidPasswordException();

            if (message == Messages.UserDisabled)
                return new UserDisabledException();

            if (message == Messages.InvalidIdpResponse)
                return new InvalidIdpResponseException();

            if (message == Messages.TooManyAttempts)
                return new TooManyAttemptsException();

            if (message == Messages.ExpiredOobCode)
                return new ExpiredOobCodeException();

            if (message == Messages.InvalidOobCode)
                return new InvalidOobCodeException();

            if (message == Messages.InvalidIdToken)
                return new InvalidIdTokenException();

            if (message == Messages.WeakPassword)
                return new WeakPasswordException();

            if (message == Messages.UserNotFound)
                return new UserNotFoundException();

            if (message == Messages.CredentialTooOld)
                return new CredentialTooOldException();

            if (message == Messages.TokenExpired)
                return new TokenExpiredException();

            if (message == Messages.FederateUserIdAlreadyLinked)
                return new FederateUserIdAlreadyLinkedException();

            return new FirebaseException();
        }
    }
}