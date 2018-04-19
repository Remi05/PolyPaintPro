using Slofth.Firebase.Http;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slofth.Firebase.Tests.Http
{
    [TestFixture]
    class ErrorTests
    {
        [TestCase("INVALID_REFRESH_TOKEN", typeof(InvalidRefreshTokenException), TestName = "Error_InvalidRefreshToken_ShouldThrowInvalidRefreshTokenException")]
        [TestCase("INVALID_GRANT_TYPE", typeof(InvalidGrantTypeException), TestName = "Error_InvalidGrantType_ShouldThrowInvalidGrantTypeException")]
        [TestCase("MISSING_REFRESH_TOKEN", typeof(MissingRefreshTokenException), TestName = "Error_MissingRefreshToken_ShouldThrowMissingRefreshTokenException")]
        [TestCase("EMAIL_EXISTS", typeof(EmailExistsException), TestName = "Error_EmailExists_ShouldThrow")]
        [TestCase("OPERATION_NOT_ALLOWED", typeof(OperationNotAllowedException), TestName = "Error_OperationNotAllowed_ShouldThrowOperationNotAllowedException")]
        [TestCase("TOO_MANY_ATTEMPTS_TRY_LATER", typeof(TooManyAttemptsException), TestName = "Error_TooManyAttempts_ShouldThrowTooManyAttemptsException")]
        [TestCase("EMAIL_NOT_FOUND", typeof(EmailNotFoundException), TestName = "Error_EmailNotFound_ShouldThrowEmailNotFoundException")]
        [TestCase("INVALID_PASSWORD", typeof(InvalidPasswordException), TestName = "Error_InvalidPassword_ShouldThrowInvalidPasswordException")]
        [TestCase("USER_DISABLED", typeof(UserDisabledException), TestName = "Error_UserDisabled_ShouldThrowUserDisabledException")]
        [TestCase("INVALID_IDP_RESPONSE", typeof(InvalidIdpResponseException), TestName = "Error_InvalidIdpResponse_ShouldThrowInvalidIdpResponseException")]
        [TestCase("EXPIRED_OOB_CODE", typeof(ExpiredOobCodeException), TestName = "Error_ExpiredOobCode_ShouldThrowExpiredOobCodeException")]
        [TestCase("INVALID_OOB_CODE", typeof(InvalidOobCodeException), TestName = "Error_InvalidOobCode_ShouldThrowInvalidOobCodeException")]
        [TestCase("INVALID_ID_TOKEN:The ", typeof(InvalidIdTokenException), TestName = "Error_InvalidIdToken_ShouldThrowInvalidIdTokenException")]
        [TestCase("WEAK_PASSWORD", typeof(WeakPasswordException), TestName = "Error_WeakPassword_ShouldThrowWeakPasswordException")]
        [TestCase("USER_NOT_FOUND", typeof(UserNotFoundException), TestName = "Error_UserNotFound_ShouldThrowUserNotFoundException")]
        [TestCase("CREDENTIAL_TOO_OLD_LOGIN_AGAIN", typeof(CredentialTooOldException), TestName = "Error_CredentialTooOldLoginAgain_ShouldThrowCredentialTooOldException")]
        [TestCase("TOKEN_EXPIRED", typeof(TokenExpiredException), TestName = "Error_TokenExpired_ShouldThrowTokenExpiredException")]
        [TestCase("FEDERATED_USER_ID_ALREADY_LINKED", typeof(FederateUserIdAlreadyLinkedException), TestName = "Error_FederateUserIdAlreadyLinked_ShouldThrowFederateUserIdAlreadyLinkedException")]
        public void Error_ShouldThrowRightException(string message, Type exceptionType)
        {
            // Arrange
            var error = new Error();
            error.message = message;

            // Assert
            Assert.IsInstanceOf(exceptionType, error.GetCorrespondingException());
        }
    }
}
