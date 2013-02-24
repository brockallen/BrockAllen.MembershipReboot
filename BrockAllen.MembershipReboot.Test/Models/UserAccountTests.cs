using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrockAllen.MembershipReboot.Test.Models
{
    [TestClass]
    public class UserAccountTests
    {
        public class MockUserAccount : Mock<UserAccount>
        {
            public MockUserAccount()
            {
                this.CallBase = true;
            }
        }

        [TestClass]
        public class VerifyAccount
        {
            [TestMethod]
            public void NullKey_VerificationFails()
            {
                var subject = new UserAccount();
                var result = subject.VerifyAccount(null);
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void EmptyKey_VerificationFails()
            {
                var subject = new UserAccount();
                var result = subject.VerifyAccount(null);
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void AlreadyVerified_VerificationFails()
            {
                var subject = new UserAccount();
                subject.IsAccountVerified = true;
                var result = subject.VerifyAccount("test");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void KeysDontMatch_VerificationFails()
            {
                var subject = new UserAccount();
                subject.IsAccountVerified = true;
                subject.VerificationKey = "test1";
                var result = subject.VerifyAccount("test2");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void KeysMatch_VerificationSucceeds()
            {
                var subject = new UserAccount();
                subject.VerificationKey = "test1";
                var result = subject.VerifyAccount("test1");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void SuccessfulVerification_VerificationFlagsReset()
            {
                var subject = new UserAccount();
                subject.VerificationKey = "test";
                subject.VerifyAccount("test");
                Assert.AreEqual(true, subject.IsAccountVerified);
                Assert.IsNull(subject.VerificationKey);
                Assert.IsNull(subject.VerificationKeySent);
            }
            [TestMethod]
            public void FailedVerification_VerificationFlagsNotChanged()
            {
                var sent = new DateTime(2000, 2, 3);
                var subject = new UserAccount();
                subject.VerificationKey = "test1";
                subject.VerificationKeySent = sent;
                
                subject.VerifyAccount("test2");
                
                Assert.AreEqual(false, subject.IsAccountVerified);
                Assert.AreEqual("test1", subject.VerificationKey);
                Assert.AreEqual(sent, subject.VerificationKeySent);
            }
        }

        [TestClass]
        public class ChangePassword
        {
            [TestMethod]
            public void AuthenticateFails_ReturnsFail()
            {
                var subject = new MockUserAccount();
                subject
                    .Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                    .Returns(false);
                var result = subject.Object.ChangePassword("old", "new", 0, TimeSpan.Zero);
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void AuthenticateSucceeds_ReturnsSuccess()
            {
                Mock<UserAccount> subject = new MockUserAccount();
                subject
                    .Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                    .Returns(true);
                var result = subject.Object.ChangePassword("old", "new", 0, TimeSpan.Zero);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void SuccessfulChangePassword_SetsPassword()
            {
                Mock<UserAccount> subject = new MockUserAccount();
                subject
                    .Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>()))
                    .Returns(true);
                var result = subject.Object.ChangePassword("old", "new", 0, TimeSpan.Zero);
                subject.Verify(x => x.SetPassword("new"));
            }
        }

        [TestClass]
        public class SetPassword
        {
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullPassword_Throws()
            {
                var subject = new UserAccount();
                subject.SetPassword(null);
            }
            
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyPassword_Throws()
            {
                var subject = new UserAccount();
                subject.SetPassword("");
            }

            [TestMethod]
            public void HashedPasswordUpdated()
            {
                var subject = new MockUserAccount();
                subject.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hash");
                subject.Object.SetPassword("pwd");
                Assert.AreEqual("hash", subject.Object.HashedPassword);
            }

            [TestMethod]
            public void PasswordChangedUpdated()
            {
                var subject = new MockUserAccount();
                var now = new DateTime(2000, 2, 3);
                subject.Setup(x => x.UtcNow).Returns(now);
                subject.Object.SetPassword("pwd");
                Assert.AreEqual(now, subject.Object.PasswordChanged);
            }
        }

        [TestClass]
        public class IsVerificationKeyStale
        {
            [TestMethod]
            public void VerificationKeySentIsNull_ReturnsTrue()
            {
                var subject = new MockUserAccount();
                var now = new DateTime(2000, 2, 3);
                subject.Setup(x => x.UtcNow).Returns(now);
                var result = subject.Object.IsVerificationKeyStale;
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void VerificationKeySentPastStaleDuration_ReturnsTrue()
            {
                var subject = new MockUserAccount();
                var now = new DateTime(2000, 2, 3);
                subject.Setup(x => x.UtcNow).Returns(now);
                subject.Object.VerificationKeySent = now.Subtract(TimeSpan.FromDays(UserAccount.VerificationKeyStaleDuration).Add(TimeSpan.FromSeconds(1)));
                var result = subject.Object.IsVerificationKeyStale;
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void VerificationKeySentAtExactStaleDurationInPast_ReturnsFalse()
            {
                var subject = new MockUserAccount();
                var now = new DateTime(2000, 2, 3);
                subject.Setup(x => x.UtcNow).Returns(now);
                subject.Object.VerificationKeySent = now.Subtract(TimeSpan.FromDays(UserAccount.VerificationKeyStaleDuration));
                var result = subject.Object.IsVerificationKeyStale;
                Assert.IsFalse(result);
            }
        }

        [TestClass]
        public class ResetPassword
        {
            [TestMethod]
            public void AccountNotVerified_ReturnsFail()
            {
                var subject = new UserAccount();
                subject.IsAccountVerified = false;
                var result = subject.ResetPassword();
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void AccountVerified_ReturnsSuccess()
            {
                var subject = new UserAccount();
                subject.IsAccountVerified = true;
                var result = subject.ResetPassword();
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void AccountVerified_VerificationKeyStale_VerificationKeyReset()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(true);
                subject.Setup(x => x.GenerateSalt()).Returns("salt");
                var result = subject.Object.ResetPassword();
                Assert.AreEqual("salt", subject.Object.VerificationKey);
            }

            [TestMethod]
            public void AccountVerified_VerificationKeyNotStale_VerificationKeyNotReset()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Object.VerificationKey = "key";
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                var result = subject.Object.ResetPassword();
                Assert.AreEqual("key", subject.Object.VerificationKey);
            }
            
            [TestMethod]
            public void AccountVerified_VerificationKeyStale_VerificationKeySentReset()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(true);
                var now = new DateTime(2000, 2, 3);
                subject.Setup(x => x.UtcNow).Returns(now);
                var result = subject.Object.ResetPassword();
                Assert.AreEqual(now, subject.Object.VerificationKeySent);
            }
            [TestMethod]
            public void AccountVerified_VerificationKeyNotStale_VerificationKeySentNotReset()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                var now = new DateTime(2000, 2, 3);
                subject.Object.VerificationKeySent = now;
                var result = subject.Object.ResetPassword();
                Assert.AreEqual(now, subject.Object.VerificationKeySent);
            }
        }

        [TestClass]
        public class ChangePasswordFromResetKey
        {
            [TestMethod]
            public void NullKey_ReturnsFail()
            {
                var subject = new UserAccount();
                var result = subject.ChangePasswordFromResetKey(null, "new");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void EmptyKey_ReturnsFail()
            {
                var subject = new UserAccount();
                var result = subject.ChangePasswordFromResetKey("", "new");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void NotVerified_ReturnsFail()
            {
                var subject = new UserAccount();
                subject.IsAccountVerified = false;
                var result = subject.ChangePasswordFromResetKey("key", "new");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void VerificationKeyStale_ReturnsFail()
            {
                var subject = new MockUserAccount();
                subject.Setup(x => x.IsVerificationKeyStale).Returns(true);
                var result = subject.Object.ChangePasswordFromResetKey("key", "new");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void KeyDoesntMatchVerificationKey_ReturnsFail()
            {
                var subject = new UserAccount();
                subject.VerificationKey = "key1";
                var result = subject.ChangePasswordFromResetKey("key2", "new");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void KeyMatchesVerificationKey_ReturnsSuccess()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                subject.Object.VerificationKey = "key";
                var result = subject.Object.ChangePasswordFromResetKey("key", "new");
                Assert.IsTrue(result);
            }
            [TestMethod]
            public void ChangeSuccess_VerificationFlagsReset()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Object.VerificationKey = "key";
                subject.Object.VerificationKeySent = new DateTime(2000, 2, 3);
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                var result = subject.Object.ChangePasswordFromResetKey("key", "new");
                Assert.IsNull(subject.Object.VerificationKey);
                Assert.IsNull(subject.Object.VerificationKeySent);
            }
            [TestMethod]
            public void ChangeSuccess_SetPasswordInvoked()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Object.VerificationKey = "key";
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                var result = subject.Object.ChangePasswordFromResetKey("key", "new");
                subject.Verify(x => x.SetPassword("new"));
            }
            
        }

        [TestClass]
        public class Authenticate
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void FailedLoginCountZero_Throws()
            {
                var sub = new UserAccount();
                sub.Authenticate("pass", 0, TimeSpan.FromMinutes(5));
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void FailedLoginCountBelowZero_Throws()
            {
                var sub = new UserAccount();
                sub.Authenticate("pass", -1, TimeSpan.FromMinutes(5));
            }

            [TestMethod]
            public void PasswordNull_ReturnsFail()
            {
                var sub = new UserAccount();
                var result = sub.Authenticate(null, 10, TimeSpan.FromMinutes(5));
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void PasswordEmpty_ReturnsFail()
            {
                var sub = new UserAccount();
                var result = sub.Authenticate("", 10, TimeSpan.FromMinutes(5));
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void AccountNotVerified_ReturnsFail()
            {
                var sub = new UserAccount();
                sub.IsAccountVerified = false;
                var result = sub.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void LoginNotAllowed_ReturnsFail()
            {
                var sub = new UserAccount();
                sub.IsAccountVerified = true;
                sub.IsLoginAllowed = false;
                var result = sub.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void TooManyRecentPasswordFailures_ReturnsFail()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = false;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(true);
                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void TooManyRecentPasswordFailures_IncrementsFailedLoginCount()
            {
                var sub = new MockUserAccount();
                sub.Object.FailedLoginCount = 3;
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = true;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(true);
                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.AreEqual(4, sub.Object.FailedLoginCount);
            }

            [TestMethod]
            public void PasswordCorrect_ReturnsSuccess()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = true;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(false);
                sub.Setup(x => x.VerifyHashedPassword("pass")).Returns(true);
                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void Success_ResetsPasswordFailureFlags()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = true;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(false);
                sub.Setup(x => x.VerifyHashedPassword("pass")).Returns(true);
                var now = new DateTime(2000, 2, 3);
                sub.Setup(x => x.UtcNow).Returns(now);
                sub.Object.FailedLoginCount = 10;
                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.AreEqual(0, sub.Object.FailedLoginCount);
                Assert.AreEqual(now, sub.Object.LastLogin);
            }

            [TestMethod]
            public void PasswordIncorrect_SetsLastFailedLogin()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = true;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(false);
                sub.Setup(x => x.VerifyHashedPassword(It.IsAny<string>())).Returns(false);
                var now = new DateTime(2000, 2, 3);
                sub.Setup(x => x.UtcNow).Returns(now);

                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.AreEqual(now, sub.Object.LastFailedLogin);
            }

            [TestMethod]
            public void PasswordIncorrect_LastFailedLoginCountIsZero_SetsLastFailedCountToOne()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = true;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(false);
                sub.Setup(x => x.VerifyHashedPassword(It.IsAny<string>())).Returns(false);
                var now = new DateTime(2000, 2, 3);
                sub.Setup(x => x.UtcNow).Returns(now);
                sub.Object.FailedLoginCount = 0;

                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.AreEqual(1, sub.Object.FailedLoginCount);
            }
            [TestMethod]
            public void PasswordIncorrect_IncrementsLastFailedCount()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.IsLoginAllowed = true;
                sub.Setup(x => x.HasTooManyRecentPasswordFailures(It.IsAny<int>(), It.IsAny<TimeSpan>()))
                   .Returns(false);
                sub.Setup(x => x.VerifyHashedPassword(It.IsAny<string>())).Returns(false);
                var now = new DateTime(2000, 2, 3);
                sub.Setup(x => x.UtcNow).Returns(now);
                sub.Object.FailedLoginCount = 3;

                var result = sub.Object.Authenticate("pass", 10, TimeSpan.FromMinutes(5));
                Assert.AreEqual(4, sub.Object.FailedLoginCount);
            }
        }

        [TestClass]
        public class HasTooManyRecentPasswordFailures
        {
            
        }
    }
}
