using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace MyClassLibrary.Test
{
    [TestClass]
    public class LoginServiceTests
    {
        const string user = "user";
        const string validPassword = "validPassword";
        const string invalidPassword = "invalidPassword";

        [TestMethod]
        public void When_CredentialsAreValid_Expect_LoginSucceeds()
        {
            // Arrange
            var mockCredentialManager = Mock.Create<ICredentialManager>();
            Mock.Arrange(() => mockCredentialManager.CheckCredentials(user, validPassword)).Returns(true).OccursOnce();
            var mockAccountLocker = Mock.Create<IAccountLocker>();
            Mock.Arrange(() => mockAccountLocker.IsLocked(user)).Returns(false);

            // Act
            var sut = new LoginService(mockCredentialManager, mockAccountLocker);
            var loginResult = sut.Login(user, validPassword);

            // Assert
            Assert.IsTrue(loginResult);
        }

        [TestMethod]
        public void When_CredentialsAreInvalid3TimesInARow_Expect_LoginFailsAndAccountIsLocked()
        {
            // Arrange
            var mockCredentialManager = Mock.Create<ICredentialManager>();
            Mock.Arrange(() => mockCredentialManager.CheckCredentials(user, validPassword)).Returns(true).OccursNever();
            Mock.Arrange(() => mockCredentialManager.CheckCredentials(user, invalidPassword)).Occurs(3);
            var mockAccountLocker = Mock.Create<IAccountLocker>();
            Mock.Arrange(() => mockAccountLocker.IsLocked(user)).Returns(false);
            Mock.Arrange(() => mockAccountLocker.Lock(user)).DoInstead(() => Mock.Arrange(() => mockAccountLocker.IsLocked(user)).Returns(true)).OccursOnce();

            // Act
            var sut = new LoginService(mockCredentialManager, mockAccountLocker);
            var loginResult1 = sut.Login(user, invalidPassword);
            var loginResult2 = sut.Login(user, invalidPassword);
            var loginResult3 = sut.Login(user, invalidPassword);
            var loginResult4 = sut.Login(user, invalidPassword);

            // Assert
            Assert.IsFalse(loginResult1 && loginResult2 && loginResult3 && loginResult4);
            Mock.Assert(mockCredentialManager);
            Mock.Assert(mockAccountLocker);
        }

        [TestMethod]
        public void When_CredentialsAreValidAndAccountIsLocked_Expect_LoginFails()
        {
            // Arrange
            var mockCredentialManager = Mock.Create<ICredentialManager>();
            Mock.Arrange(() => mockCredentialManager.CheckCredentials(user, validPassword)).OccursNever();
            var mockAccountLocker = Mock.Create<IAccountLocker>();
            Mock.Arrange(() => mockAccountLocker.IsLocked(user)).Returns(true);

            // Act
            var sut = new LoginService(mockCredentialManager, mockAccountLocker);
            var loginResult = sut.Login(user, validPassword);

            // Assert
            Assert.IsFalse(loginResult);
            Mock.Assert(mockCredentialManager);
            Mock.Assert(mockAccountLocker);
        }

        [TestMethod]
        public void When_CredentialsAreValidAfterAccountLockout_Expect_LoginSucceeds()
        {
            // Arrange
            var mockCredentialManager = Mock.Create<ICredentialManager>();
            Mock.Arrange(() => mockCredentialManager.CheckCredentials(user, validPassword)).Returns(true).OccursOnce();
            var mockAccountLocker = Mock.Create<IAccountLocker>();
            Mock.Arrange(() => mockAccountLocker.IsLocked(user)).Returns(true);
            mockAccountLocker.LockTimeoutElapsedEvent += delegate (object sender, AccountElapsedEventArgs e)
            {
                Mock.Arrange(() => mockAccountLocker.IsLocked(e.UserName)).Returns(false);
            };

            // Act
            var sut = new LoginService(mockCredentialManager, mockAccountLocker);
            var loginResult1 = sut.Login(user, invalidPassword);
            // simulate that timeout has expired
            Mock.Raise(() => mockAccountLocker.LockTimeoutElapsedEvent += null, new AccountElapsedEventArgs(user));
            var loginResult2 = sut.Login(user, validPassword);

            // Assert
            Assert.IsFalse(loginResult1);
            Assert.IsTrue(loginResult2);
            Mock.Assert(mockCredentialManager);
            Mock.Assert(mockAccountLocker);
        }
    }
}
