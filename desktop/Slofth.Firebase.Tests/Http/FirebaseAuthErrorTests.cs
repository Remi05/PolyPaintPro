using Slofth.Firebase.Auth;
using Slofth.Firebase.Database;
using Slofth.Firebase.Http;
using NUnit.Framework;
using System;

namespace Slofth.Firebase.Tests.Http
{
    [TestFixture]
    class FirebaseErrorTests
    {
        [TestCase("INVALID_REFRESH_TOKEN", typeof(InvalidRefreshTokenException), TestName = "FirebaseAuthError_InvalidRefreshToken_ShouldThrowInvalidRefreshTokenException")]
        [TestCase("INVALID_GRANT_TYPE", typeof(InvalidGrantTypeException), TestName = "FirebaseAuthError_InvalidGrantType_ShouldThrowInvalidGrantTypeException")]
        [TestCase("MISSING_REFRESH_TOKEN", typeof(MissingRefreshTokenException), TestName = "FirebaseAuthError_MissingRefreshToken_ShouldThrowMissingRefreshTokenException")]
        [TestCase("EMAIL_EXISTS", typeof(EmailExistsException), TestName = "FirebaseAuthError_EmailExists_ShouldThrow")]
        [TestCase("OPERATION_NOT_ALLOWED", typeof(OperationNotAllowedException), TestName = "FirebaseAuthError_OperationNotAllowed_ShouldThrowOperationNotAllowedException")]
        [TestCase("TOO_MANY_ATTEMPTS_TRY_LATER", typeof(TooManyAttemptsException), TestName = "Error_TooManyAttempts_ShouldThrowTooManyAttemptsException")]
        [TestCase("EMAIL_NOT_FOUND", typeof(EmailNotFoundException), TestName = "FirebaseAuthError_EmailNotFound_ShouldThrowEmailNotFoundException")]
        [TestCase("INVALID_PASSWORD", typeof(InvalidPasswordException), TestName = "FirebaseAuthError_InvalidPassword_ShouldThrowInvalidPasswordException")]
        [TestCase("USER_DISABLED", typeof(UserDisabledException), TestName = "FirebaseAuthError_UserDisabled_ShouldThrowUserDisabledException")]
        [TestCase("INVALID_IDP_RESPONSE", typeof(InvalidIdpResponseException), TestName = "FirebaseAuthError_InvalidIdpResponse_ShouldThrowInvalidIdpResponseException")]
        [TestCase("EXPIRED_OOB_CODE", typeof(ExpiredOobCodeException), TestName = "FirebaseAuthError_ExpiredOobCode_ShouldThrowExpiredOobCodeException")]
        [TestCase("INVALID_OOB_CODE", typeof(InvalidOobCodeException), TestName = "FirebaseAuthError_InvalidOobCode_ShouldThrowInvalidOobCodeException")]
        [TestCase("INVALID_ID_TOKEN:The ", typeof(InvalidIdTokenException), TestName = "FirebaseAuthError_InvalidIdToken_ShouldThrowInvalidIdTokenException")]
        [TestCase("WEAK_PASSWORD", typeof(WeakPasswordException), TestName = "FirebaseAuthError_WeakPassword_ShouldThrowWeakPasswordException")]
        [TestCase("USER_NOT_FOUND", typeof(UserNotFoundException), TestName = "FirebaseAuthError_UserNotFound_ShouldThrowUserNotFoundException")]
        [TestCase("CREDENTIAL_TOO_OLD_LOGIN_AGAIN", typeof(CredentialTooOldException), TestName = "FirebaseAuthError_CredentialTooOldLoginAgain_ShouldThrowCredentialTooOldException")]
        [TestCase("TOKEN_EXPIRED", typeof(TokenExpiredException), TestName = "FirebaseAuthError_TokenExpired_ShouldThrowTokenExpiredException")]
        [TestCase("FEDERATED_USER_ID_ALREADY_LINKED", typeof(FederateUserIdAlreadyLinkedException), TestName = "FirebaseAuthError_FederateUserIdAlreadyLinked_ShouldThrowFederateUserIdAlreadyLinkedException")]
        [TestCase("UNKNOWN_ERROR_THAT_DOESNT_EXIST", typeof(FirebaseAuthException), TestName = "FirebaseAuthError_UnknownError_ShouldThrowFirebaseAuthException")]
        public void FirebaseAuthError_ShouldThrowRightException(string message, Type exceptionType)
        {
            // Arrange
            var error = new FirebaseAuthError();
            error.Message = message;

            // Assert
            Assert.IsInstanceOf(exceptionType, error.GetCorrespondingException());
        }

        [TestCase("Could not parse auth token.", typeof(CouldNotParseAuthTokenException), TestName = "Error_CouldNotParseAuthToken_ShouldThrowCouldNotParseAuthTokenException")]
        [TestCase("Erreur qui n'existe pas.", typeof(FirebaseDatabaseException), TestName = "FirebaseAuthError_FirebaseDatabase_ShouldThrowFirebaseDatabaseException")]
        public void Error_ShouldThrowRightException(string message, Type exceptionType)
        {
            // Arrange
            var error = new FirebaseDatabaseError();
            error.Error = message;

            // Assert
            Assert.IsInstanceOf(exceptionType, error.GetCorrespondingException());
        }
    }
}
