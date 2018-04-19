using Slofth.Firebase.Auth;
using Newtonsoft.Json;
using System;

namespace Slofth.Firebase.Http
{
    internal class FirebaseAuthError : IFirebaseError
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

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public Exception GetCorrespondingException()
        {
            if (Message == Messages.InvalidRefreshToken)
                return new InvalidRefreshTokenException();

            if (Message == Messages.InvalidGrantType)
                return new InvalidGrantTypeException();

            if (Message == Messages.MissingRefreshToken)
                return new MissingRefreshTokenException();

            if (Message == Messages.EmailExists)
                return new EmailExistsException();

            if (Message == Messages.OperationNotAllowed)
                return new OperationNotAllowedException();

            if (Message == Messages.TooManyAttempts)
                return new TooManyAttemptsException();

            if (Message == Messages.EmailNotFound)
                return new EmailNotFoundException();

            if (Message == Messages.InvalidPassword)
                return new InvalidPasswordException();

            if (Message == Messages.UserDisabled)
                return new UserDisabledException();

            if (Message == Messages.InvalidIdpResponse)
                return new InvalidIdpResponseException();

            if (Message == Messages.TooManyAttempts)
                return new TooManyAttemptsException();

            if (Message == Messages.ExpiredOobCode)
                return new ExpiredOobCodeException();

            if (Message == Messages.InvalidOobCode)
                return new InvalidOobCodeException();

            if (Message == Messages.InvalidIdToken)
                return new InvalidIdTokenException();

            if (Message == Messages.WeakPassword)
                return new WeakPasswordException();

            if (Message == Messages.UserNotFound)
                return new UserNotFoundException();

            if (Message == Messages.CredentialTooOld)
                return new CredentialTooOldException();

            if (Message == Messages.TokenExpired)
                return new TokenExpiredException();

            if (Message == Messages.FederateUserIdAlreadyLinked)
                return new FederateUserIdAlreadyLinkedException();

            return new FirebaseAuthException();
        }
    }
}