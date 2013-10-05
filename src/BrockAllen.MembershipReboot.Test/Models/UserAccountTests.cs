/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrockAllen.MembershipReboot.Test.Models
{
    [TestClass]
    public class UserAccountTests
    {
        [TestClass]
        public class Init
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullTenant_Throws()
            {
                var sub = new UserAccount();
                sub.Init(null, "user", "pass", "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullUsername_Throws()
            {
                var sub = new UserAccount();
                sub.Init("ten", null, "pass", "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullPass_Throws()
            {
                var sub = new UserAccount();
                sub.Init("ten", "user", null, "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullEmail_Throws()
            {
                var sub = new UserAccount();
                sub.Init("ten", "user", "pass", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmptyTenant_Throws()
            {
                var sub = new UserAccount();
                sub.Init("", "user", "pass", "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyUsername_Throws()
            {
                var sub = new UserAccount();
                sub.Init("ten", "", "pass", "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyPass_Throws()
            {
                var sub = new UserAccount();
                sub.Init("ten", "user", "", "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyEmail_Throws()
            {
                var sub = new UserAccount();
                sub.Init("ten", "user", "pass", "");
            }
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public void ExistingID_Throws()
            {
                var sub = new UserAccount();
                sub.ID = Guid.NewGuid();
                sub.Init("ten", "user", "pass", "email");
            }

            [TestMethod]
            public void BasicProperties_Assigned()
            {
                var now = new DateTime(2000, 2, 3, 8, 30, 0);
                var sub = new MockUserAccount("ten", "user", "pass", "email", now);
                Assert.AreEqual("ten", sub.Object.Tenant);
                Assert.AreEqual("user", sub.Object.Username);
                Assert.AreEqual("email", sub.Object.Email);
                Assert.AreEqual(now, sub.Object.Created);
                Assert.AreNotEqual(Guid.Empty, sub.Object.ID);
                Assert.IsFalse(sub.Object.IsLoginAllowed);
                Assert.IsFalse(sub.Object.IsAccountVerified);
                Assert.IsNotNull(sub.Object.VerificationKey);
                Assert.AreEqual(VerificationKeyPurpose.VerifyAccount, sub.Object.VerificationPurpose);
                Assert.IsTrue(sub.Object.VerifyHashedPassword("pass"));
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
                subject.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                subject.VerificationKey = "test1";
                var result = subject.VerifyAccount("test2");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void KeysMatch_VerificationSucceeds()
            {
                var subject = new UserAccount();
                subject.VerificationKey = "test1";
                subject.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                var result = subject.VerifyAccount("test1");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void SuccessfulVerification_VerificationFlagsReset()
            {
                var subject = new UserAccount();
                subject.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
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
                subject.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                subject.VerificationKey = "test1";
                subject.VerificationKeySent = sent;

                subject.VerifyAccount("test2");

                Assert.AreEqual(false, subject.IsAccountVerified);
                Assert.AreEqual("test1", subject.VerificationKey);
                Assert.AreEqual(sent, subject.VerificationKeySent);
            }

            [TestMethod]
            public void VerificationPurposeIsNotVerifyAccount_Fails()
            {
                var sub = new MockUserAccount("t1", "user", "pass", "email");
                sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangePassword;
                
                var result = sub.Object.VerifyAccount(sub.Object.VerificationKey);

                Assert.IsFalse(result);
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

            //[TestMethod]
            //public void VerificationKeySentPastStaleDuration_ReturnsTrue()
            //{
            //    var subject = new MockUserAccount();
            //    var now = new DateTime(2000, 2, 3);
            //    subject.Setup(x => x.UtcNow).Returns(now);
            //    subject.Object.VerificationKeySent = now.Subtract(TimeSpan.FromDays(UserAccount.VerificationKeyStaleDurationDays).Add(TimeSpan.FromSeconds(1)));
            //    var result = subject.Object.IsVerificationKeyStale;
            //    Assert.IsTrue(result);
            //}

            //[TestMethod]
            //public void VerificationKeySentAtExactStaleDurationInPast_ReturnsFalse()
            //{
            //    var subject = new MockUserAccount();
            //    var now = new DateTime(2000, 2, 3);
            //    subject.Setup(x => x.UtcNow).Returns(now);
            //    subject.Object.VerificationKeySent = now.Subtract(TimeSpan.FromDays(UserAccount.VerificationKeyStaleDurationDays));
            //    var result = subject.Object.IsVerificationKeyStale;
            //    Assert.IsFalse(result);
            //}
        }

        [TestClass]
        public class ResetPassword
        {
            //[TestMethod]
            //public void AccountNotVerified_RaisesAccountCreatedEvent()
            //{
            //    var acct = new MockUserAccount();

            //    acct.Object.ResetPassword();

            //    var es = acct.Object as IEventSource;
            //    Assert.IsTrue(es.Events.Where(x => x is AccountCreatedEvent).Any());
            //}
            
            //[TestMethod]
            //public void AccountVerified_RaisesPasswordResetRequestedEvent()
            //{
            //    var acct = new MockUserAccount();
            //    acct.VerifyAccount();

            //    acct.Object.ResetPassword();

            //    var es = acct.Object as IEventSource;
            //    Assert.IsTrue(es.Events.Where(x => x is PasswordResetRequestedEvent).Any());
            //}

            [TestMethod]
            public void VerificationKeyIsNull_SetsVerificationKey()
            {
                var acct = new MockUserAccount();
                acct.VerifyAccount();
                
                acct.Object.ResetPassword();

                Assert.IsNotNull(acct.Object.VerificationKey);
                Assert.AreEqual(VerificationKeyPurpose.ChangePassword, acct.Object.VerificationPurpose);
            }
            [TestMethod]
            public void VerificationKeyIsOld_SetsVerificationKey()
            {
                var acct = new MockUserAccount();
                acct.Object.IsAccountVerified = true;
                acct.Object.VerificationPurpose = VerificationKeyPurpose.ChangePassword;
                acct.Object.VerificationKeySent = DateTime.Now.AddYears(-1);
                acct.Object.VerificationKey = "foo";
                
                acct.Object.ResetPassword();

                Assert.AreNotEqual("foo", acct.Object.VerificationKey);
                Assert.IsNotNull(acct.Object.VerificationKey);
                Assert.AreEqual(VerificationKeyPurpose.ChangePassword, acct.Object.VerificationPurpose);
            }
            [TestMethod]
            public void VerificationPurposeNotChangePassword_CallsSetVerificationKey()
            {
                var acct = new MockUserAccount();
                acct.Object.IsAccountVerified = true;
                acct.Object.SetVerificationKey(VerificationKeyPurpose.ChangeEmail);

                acct.Object.ResetPassword();

                Assert.IsNotNull(acct.Object.VerificationKey);
                Assert.AreEqual(VerificationKeyPurpose.ChangePassword, acct.Object.VerificationPurpose);
            }

            [TestMethod]
            public void VerificationNotStale_DoesNotCallsSetVerificationKey()
            {
                var acct = new MockUserAccount();
                acct.Object.IsAccountVerified = true;
                acct.Object.SetVerificationKey(VerificationKeyPurpose.ChangePassword);
                acct.Object.VerificationKey = "foo";

                acct.Object.ResetPassword();

                Assert.AreEqual("foo", acct.Object.VerificationKey);
                Assert.AreEqual(VerificationKeyPurpose.ChangePassword, acct.Object.VerificationPurpose);
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
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(true);
                var result = subject.Object.ChangePasswordFromResetKey("key", "new");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void PurposeDoesntMatch_ReturnsFail()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                subject.Object.VerificationKey = "key1";
                subject.Object.VerificationPurpose = null;
                var result = subject.Object.ChangePasswordFromResetKey("key2", "new");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void KeyDoesntMatchVerificationKey_ReturnsFail()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Setup(x => x.IsVerificationKeyStale).Returns(false);
                subject.Object.VerificationKey = "key1";
                subject.Object.VerificationPurpose = VerificationKeyPurpose.ChangePassword;
                var result = subject.Object.ChangePasswordFromResetKey("key2", "new");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void KeyMatchesVerificationKey_ReturnsSuccess()
            {
                var subject = new MockUserAccount();
                subject.Object.IsAccountVerified = true;
                subject.Object.VerificationPurpose = VerificationKeyPurpose.ChangePassword;
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
                subject.Object.VerificationPurpose = VerificationKeyPurpose.ChangePassword;
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
                subject.Object.VerificationPurpose = VerificationKeyPurpose.ChangePassword;
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
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void FailedLoginCountZero_Throws()
            {
                var sub = new UserAccount();
                sub.HasTooManyRecentPasswordFailures(0, TimeSpan.FromMinutes(5));
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void FailedLoginCountBelowZero_Throws()
            {
                var sub = new UserAccount();
                sub.HasTooManyRecentPasswordFailures(-1, TimeSpan.FromMinutes(5));
            }

            [TestMethod]
            public void FailedLoginCountLow_ReturnsFalse()
            {
                var sub = new MockUserAccount();
                sub.Object.FailedLoginCount = 2;
                var duration = TimeSpan.FromMinutes(10);
                var result = sub.Object.HasTooManyRecentPasswordFailures(5, duration);
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void FailedLoginCountHigh_LastFailedLoginIsRecent_ReturnsTrue()
            {
                var sub = new MockUserAccount();
                sub.Object.FailedLoginCount = 10;

                var date = new DateTime(2003, 2, 3, 8, 30, 0);
                sub.Setup(x => x.UtcNow).Returns(date);
                sub.Object.LastFailedLogin = date.Subtract(TimeSpan.FromMinutes(1));
                var duration = TimeSpan.FromMinutes(10);

                var result = sub.Object.HasTooManyRecentPasswordFailures(5, duration);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void FailedLoginCountHigh_LastFailedLoginIsExactDuration_ReturnsTrue()
            {
                var sub = new MockUserAccount();
                sub.Object.FailedLoginCount = 10;

                var date = new DateTime(2003, 2, 3, 8, 30, 0);
                sub.Setup(x => x.UtcNow).Returns(date);
                var duration = TimeSpan.FromMinutes(10);
                sub.Object.LastFailedLogin = date.Subtract(duration);

                var result = sub.Object.HasTooManyRecentPasswordFailures(5, duration);
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void FailedLoginCountHigh_LastFailedLoginIsNotRecent_ReturnsFalse()
            {
                var sub = new MockUserAccount();
                sub.Object.FailedLoginCount = 10;

                var date = new DateTime(2003, 2, 3, 8, 30, 0);
                sub.Setup(x => x.UtcNow).Returns(date);
                var duration = TimeSpan.FromMinutes(10);
                sub.Object.LastFailedLogin = date.Subtract(TimeSpan.FromMinutes(11));

                var result = sub.Object.HasTooManyRecentPasswordFailures(5, duration);
                Assert.IsFalse(result);
            }
        }

        [TestClass]
        public class ChangeEmailRequest
        {
            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void NewEmailIsNull_Throws()
            //{
            //    var sub = new UserAccount();
            //    var result = sub.ChangeEmailRequest(null);
            //}
            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void EmptyEmailIsNull_Throws()
            //{
            //    var sub = new UserAccount();
            //    var result = sub.ChangeEmailRequest("");
            //}


            //[Ignore]
            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void MalFormedEmailIsNull_Throws()
            //{
            //    var sub = new UserAccount();
            //    var result = sub.ChangeEmailRequest("test");
            //}

            //[TestMethod]
            //public void AccountNotVerified_ReturnsFail()
            //{
            //    var sub = new UserAccount();
            //    sub.IsAccountVerified = false;
            //    var result = sub.ChangeEmailRequest("test@test.com");
            //    Assert.IsFalse(result);
            //}

            //[TestMethod]
            //public void AccountVerified_ReturnsSuccess()
            //{
            //    var sub = new UserAccount();
            //    sub.IsAccountVerified = true;
            //    var result = sub.ChangeEmailRequest("test@test.com");
            //    Assert.IsTrue(result);
            //}

            //[TestMethod]
            //public void ChangeEmailSuccess_VerificationKeyStale_VerificationKeyFlagsReset()
            //{
            //    var sub = new MockUserAccount();
            //    sub.Object.IsAccountVerified = true;
            //    sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
            //    sub.Setup(x => x.IsVerificationKeyStale).Returns(true);
            //    sub.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");
            //    sub.Setup(x => x.GenerateSalt()).Returns("salt");
            //    var now = new DateTime(2000, 2, 3);
            //    sub.Setup(x => x.UtcNow).Returns(now);

            //    var result = sub.Object.ChangeEmailRequest("test@test.com");
            //    Assert.AreEqual("hashsalt", sub.Object.VerificationKey);
            //    Assert.AreEqual(now, sub.Object.VerificationKeySent);
            //}

            //[TestMethod]
            //public void ChangeEmailSuccess_VerificationPurposeMismatch_VerificationKeyFlagsReset()
            //{
            //    var sub = new MockUserAccount();
            //    sub.Object.IsAccountVerified = true;
            //    sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
            //    sub.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");
            //    sub.Setup(x => x.GenerateSalt()).Returns("salt");
            //    var now = new DateTime(2000, 2, 3);
            //    sub.Setup(x => x.UtcNow).Returns(now);

            //    var result = sub.Object.ChangeEmailRequest("test@test.com");
            //    Assert.AreEqual("hashsalt", sub.Object.VerificationKey);
            //    Assert.AreEqual(now, sub.Object.VerificationKeySent);
            //}

            //[TestMethod]
            //public void ChangeEmailSuccess_VerificationKeyDoesntMatchEmailPrefix_VerificationKeyFlagsReset()
            //{
            //    var sub = new MockUserAccount();
            //    sub.Object.IsAccountVerified = true;
            //    sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
            //    sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
            //    sub.Setup(x => x.Hash(It.IsAny<string>())).Returns("hash");
            //    sub.Setup(x => x.GenerateSalt()).Returns("salt");
            //    var now = new DateTime(2000, 2, 3);
            //    sub.Setup(x => x.UtcNow).Returns(now);
            //    sub.Object.VerificationKey = "key";
            //    sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;

            //    var result = sub.Object.ChangeEmailRequest("test@test.com");
            //    Assert.AreEqual("hashsalt", sub.Object.VerificationKey);
            //    Assert.AreEqual(now, sub.Object.VerificationKeySent);
            //}

            //[TestMethod]
            //public void ChangeEmailSuccess_VerificationKeyMatchesEmailPrefix_VerificationKeyFlagsNotReset()
            //{
            //    var sub = new MockUserAccount();
            //    sub.Object.IsAccountVerified = true;
            //    sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
            //    sub.Setup(x => x.Hash(It.IsAny<string>())).Returns("key");
            //    sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
            //    sub.Object.VerificationKey = "key";
            //    var date = new DateTime(2000, 2, 3);
            //    sub.Object.VerificationKeySent = date;

            //    var result = sub.Object.ChangeEmailRequest("test@test.com");
            //    Assert.AreEqual("key", sub.Object.VerificationKey);
            //    Assert.AreEqual(date, sub.Object.VerificationKeySent);
            //}
        }

        [TestClass]
        public class ChangeEmailFromKey
        {
            [TestMethod]
            public void NullKey_ReturnsFail()
            {
                var sub = new UserAccount();
                var result = sub.ChangeEmailFromKey(null, "new@test.com");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void EmptyKey_ReturnsFail()
            {
                var sub = new UserAccount();
                var result = sub.ChangeEmailFromKey("", "new@test.com");
                Assert.IsFalse(result);
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullEmail_Throws()
            {
                var sub = new UserAccount();
                var result = sub.ChangeEmailFromKey("key", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyEmail_Throws()
            {
                var sub = new UserAccount();
                var result = sub.ChangeEmailFromKey("key", "");
            }
            [TestMethod]
            public void VerificationKeyStale_ReturnsFail()
            {
                var sub = new MockUserAccount();
                sub.Setup(x => x.IsVerificationKeyStale).Returns(true);
                var result = sub.Object.ChangeEmailFromKey("key", "new@test.com");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void PurposeDoesNotMatch_ReturnsFail()
            {
                var sub = new MockUserAccount();
                sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
                sub.Object.VerificationKey = "key1";
                var result = sub.Object.ChangeEmailFromKey("key2", "new@test.com");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void KeyDoesNotMatch_ReturnsFail()
            {
                var sub = new MockUserAccount();
                sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
                sub.Object.VerificationKey = "key1";
                sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
                var result = sub.Object.ChangeEmailFromKey("key2", "new@test.com");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void KeyDoesNotHaveEmailPrefix_ReturnsFail()
            {
                var sub = new MockUserAccount();
                sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
                sub.Object.VerificationKey = "key";
                sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
                sub.Setup(x => x.Hash(It.IsAny<string>())).Returns("prefix");

                var result = sub.Object.ChangeEmailFromKey("key", "new@test.com");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void KeyHasEmailPrefix_ReturnsSuccess()
            {
                var sub = new MockUserAccount();
                sub.Setup(x => x.IsVerificationKeyStale).Returns(false);
                sub.Object.VerificationPurpose = VerificationKeyPurpose.ChangeEmail;
                sub.Object.VerificationKey = "prefixkey";
                sub.Setup(x => x.Hash(It.IsAny<string>())).Returns("prefix");

                var result = sub.Object.ChangeEmailFromKey("prefixkey", "new@test.com");
                Assert.IsTrue(result);
            }

            [TestMethod]
            public void ChangeEmailFromKeySuccess_SetsNewEmail()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.ChangeEmailRequest("new@test.com");
                var key = sub.Object.VerificationKey;

                var result = sub.Object.ChangeEmailFromKey(key, "new@test.com");
                Assert.AreEqual("new@test.com", sub.Object.Email);
            }

            [TestMethod]
            public void ChangeEmailFromKeySuccess_VerificationKeysReset()
            {
                var sub = new MockUserAccount();
                sub.Object.IsAccountVerified = true;
                sub.Object.ChangeEmailRequest("new@test.com");
                var key = sub.Object.VerificationKey;
                sub.Object.ChangeEmailFromKey(key, "new@test.com");

                Assert.IsNull(sub.Object.VerificationKey);
                Assert.IsNull(sub.Object.VerificationPurpose);
                Assert.IsNull(sub.Object.VerificationKeySent);
            }

        }

        [TestClass]
        public class HasClaim_1
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.HasClaim(null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.HasClaim("");
            }

            [TestMethod]
            public void ClaimTypeNotInList_ReturnsFalse()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[] { };
                var result = sub.HasClaim("type");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void ClaimTypeInList_ReturnsTrue()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[] { new UserClaim { Type = "type" } };
                var result = sub.HasClaim("type");
                Assert.IsTrue(result);
            }


        }
        [TestClass]
        public class HasClaim_2
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.HasClaim(null, "val");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.HasClaim("", "val");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullValue_Throws()
            {
                var sub = new UserAccount();
                sub.HasClaim("type", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyValue_Throws()
            {
                var sub = new UserAccount();
                sub.HasClaim("type", "");
            }

            [TestMethod]
            public void ClaimTypeNotInList_ReturnsFalse()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[] { };
                var result = sub.HasClaim("type", "value");
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void ClaimTypeInList_ReturnsTrue()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[] { new UserClaim { Type = "type", Value = "value" } };
                var result = sub.HasClaim("type", "value");
                Assert.IsTrue(result);
            }


        }

        [TestClass]
        public class GetClaimValues
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.GetClaimValues(null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.GetClaimValues("");
            }
            [TestMethod]
            public void TypeNotInList_ReturnsEmptyCollection()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[]{
                    new UserClaim{Type="type1", Value="a"},
                    new UserClaim{Type="type1", Value="b"},
                    new UserClaim{Type="type2", Value="c"},
                };
                var result = sub.GetClaimValues("type");
                Assert.AreEqual(0, result.Count());
            }
            [TestMethod]
            public void TypeInList_ReturnsCurrentValues()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[]{
                    new UserClaim{Type="type1", Value="a"},
                    new UserClaim{Type="type1", Value="b"},
                    new UserClaim{Type="type2", Value="c"},
                };
                var result = sub.GetClaimValues("type1");
                Assert.AreEqual(2, result.Count());
                CollectionAssert.AreEquivalent(new string[] { "a", "b" }, result.ToArray());
            }
        }

        [TestClass]
        public class GetClaimValue
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.GetClaimValue(null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.GetClaimValue("");
            }
            [TestMethod]
            public void TypeNotInList_ReturnsNull()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[]{
                    new UserClaim{Type="type1", Value="a"},
                    new UserClaim{Type="type1", Value="b"},
                    new UserClaim{Type="type2", Value="c"},
                };
                var result = sub.GetClaimValue("type");
                Assert.IsNull(result);
            }
            [TestMethod]
            public void TypeInList_ReturnsValue()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[]{
                    new UserClaim{Type="type1", Value="a"},
                    new UserClaim{Type="type1", Value="b"},
                    new UserClaim{Type="type2", Value="c"},
                };
                var result = sub.GetClaimValue("type2");
                Assert.AreEqual("c", result);
            }
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void MultipleTypeInList_Throws()
            {
                var sub = new UserAccount();
                sub.Claims = new UserClaim[]{
                    new UserClaim{Type="type1", Value="a"},
                    new UserClaim{Type="type1", Value="b"},
                    new UserClaim{Type="type2", Value="c"},
                };
                var result = sub.GetClaimValue("type1");
            }
        }

        [TestClass]
        public class AddClaim
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.AddClaim(null, "value");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.AddClaim("", "value");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullValue_Throws()
            {
                var sub = new UserAccount();
                sub.AddClaim("type", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyValue_Throws()
            {
                var sub = new UserAccount();
                sub.AddClaim("type", "");
            }

            [TestMethod]
            public void AlreadyHasClaim_ShouldNotAddClaim()
            {
                var sub = new MockUserAccount();
                sub.Object.Claims = new List<UserClaim>();
                sub.Setup(x => x.HasClaim(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                sub.Object.AddClaim("type", "value");

                Assert.AreEqual(0, sub.Object.Claims.Count);
            }
            [TestMethod]
            public void DoesNotHaveClaim_ShouldAddClaim()
            {
                var sub = new MockUserAccount();
                sub.Object.Claims = new List<UserClaim>();
                sub.Setup(x => x.HasClaim(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

                sub.Object.AddClaim("type", "value");

                Assert.AreEqual(1, sub.Object.Claims.Count);
                Assert.AreEqual("type", sub.Object.Claims.First().Type);
                Assert.AreEqual("value", sub.Object.Claims.First().Value);
            }
        }

        [TestClass]
        public class RemoveClaim_1
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.RemoveClaim(null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.RemoveClaim("");
            }
            [TestMethod]
            public void ClaimNotFound_RemovesNoClaims()
            {
                var sub = new UserAccount();
                sub.Claims = new List<UserClaim>()
                {
                    new UserClaim{Type="type1", Value = "value"}
                };
                sub.RemoveClaim("type2");

                Assert.AreEqual(1, sub.Claims.Count);
            }
            [TestMethod]
            public void ClaimFound_RemovesClaims()
            {
                var sub = new UserAccount();
                sub.Claims = new List<UserClaim>()
                {
                    new UserClaim{Type="type1", Value = "value"},
                    new UserClaim{Type="type2", Value = "value"},
                };
                sub.RemoveClaim("type1");

                Assert.AreEqual(1, sub.Claims.Count);
                Assert.AreEqual("type2", sub.Claims.First().Type);
                Assert.AreEqual("value", sub.Claims.First().Value);
            }
            [TestMethod]
            public void MutipleClaimsFound_RemovesClaims()
            {
                var sub = new UserAccount();
                sub.Claims = new List<UserClaim>()
                {
                    new UserClaim{Type="type1", Value = "value1"},
                    new UserClaim{Type="type1", Value = "value2"},
                    new UserClaim{Type="type2", Value = "value"},
                };
                sub.RemoveClaim("type1");

                Assert.AreEqual(1, sub.Claims.Count);
                Assert.AreEqual("type2", sub.Claims.First().Type);
                Assert.AreEqual("value", sub.Claims.First().Value);
            }
        }
        [TestClass]
        public class RemoveClaim_2
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullType_Throws()
            {
                var sub = new UserAccount();
                sub.RemoveClaim(null, "value");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyType_Throws()
            {
                var sub = new UserAccount();
                sub.RemoveClaim("", "value");
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void NullValue_Throws()
            {
                var sub = new UserAccount();
                sub.RemoveClaim("type", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void EmptyValue_Throws()
            {
                var sub = new UserAccount();
                sub.RemoveClaim("type", "");
            }

            [TestMethod]
            public void ClaimNotFound_RemovesNoClaims()
            {
                var sub = new UserAccount();
                sub.Claims = new List<UserClaim>()
                {
                    new UserClaim{Type="type1", Value = "value1"}
                };
                sub.RemoveClaim("type1", "value2");
                sub.RemoveClaim("type2", "value1");

                Assert.AreEqual(1, sub.Claims.Count);
            }

            [TestMethod]
            public void ClaimFound_RemovesClaims()
            {
                var sub = new UserAccount();
                sub.Claims = new List<UserClaim>()
                {
                    new UserClaim{Type="type1", Value = "value1"},
                    new UserClaim{Type="type2", Value = "value2"},
                };
                sub.RemoveClaim("type1", "value1");

                Assert.AreEqual(1, sub.Claims.Count);
                Assert.AreEqual("type2", sub.Claims.First().Type);
                Assert.AreEqual("value2", sub.Claims.First().Value);
            }
            [TestMethod]
            public void MutipleClaimsFound_RemovesClaims()
            {
                var sub = new UserAccount();
                sub.Claims = new List<UserClaim>()
                {
                    new UserClaim{Type="type1", Value = "value1"},
                    new UserClaim{Type="type1", Value = "value1"},
                    new UserClaim{Type="type2", Value = "value2"},
                };
                sub.RemoveClaim("type1", "value1");

                Assert.AreEqual(1, sub.Claims.Count);
                Assert.AreEqual("type2", sub.Claims.First().Type);
                Assert.AreEqual("value2", sub.Claims.First().Value);
            }
        }

        [TestClass]
        public class CloseAccount
        {
            [TestMethod]
            public void SetsClosedFields()
            {
                var sub = new MockUserAccount();
                sub.Object.IsLoginAllowed = true;
                var now = new DateTime(2000, 2, 3, 8, 30, 0);
                sub.Setup(x => x.UtcNow).Returns(now);
                sub.Object.CloseAccount();

                Assert.AreEqual(now, sub.Object.AccountClosed);
                Assert.AreEqual(true, sub.Object.IsAccountClosed);
                Assert.AreEqual(false, sub.Object.IsLoginAllowed);
            }
        }

        [TestClass]
        public class IsPasswordExpired
        {
            [TestInitialize]
            public void Init()
            {
                SecuritySettings.Instance = new SecuritySettings();
            }

            [TestMethod]
            public void NoPasswordResetFrequency_IsNeverExpired()
            {
                var subject = new MockUserAccount();
                subject.Setup(x => x.UtcNow).Returns(new DateTime(2000, 3, 10, 0, 8, 0));

                subject.Object.PasswordChanged = new DateTime(2000, 3, 9, 0, 8, 0);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(0));

                subject.Object.PasswordChanged = new DateTime(2000, 2, 9, 0, 8, 0);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(0));

                subject.Object.PasswordChanged = new DateTime(1988, 2, 9, 0, 8, 0);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(0));
            }

            [TestMethod]
            public void NegativePasswordResetFrequency_IsNeverExpired()
            {
                var subject = new MockUserAccount();
                subject.Setup(x => x.UtcNow).Returns(new DateTime(2000, 3, 10, 0, 8, 0));

                subject.Object.PasswordChanged = new DateTime(2000, 3, 9, 0, 8, 0);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(-1));

                subject.Object.PasswordChanged = new DateTime(2000, 2, 9, 0, 8, 0);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(-1));

                subject.Object.PasswordChanged = new DateTime(1988, 2, 9, 0, 8, 0);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(-1));
            }

            [TestMethod]
            public void SetPasswordResetFrequency_IsExpiredAfterDuration()
            {
                var subject = new MockUserAccount();
                var now = new DateTime(2000, 3, 10, 0, 8, 0);
                subject.Setup(x => x.UtcNow).Returns(now);

                subject.Object.PasswordChanged = now.AddDays(-5);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(30));

                subject.Object.PasswordChanged = now.AddDays(-29);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(30));

                subject.Object.PasswordChanged = now.AddDays(-30).AddSeconds(1);
                Assert.IsFalse(subject.Object.GetIsPasswordExpired(30));

                subject.Object.PasswordChanged = now.AddDays(-30);
                Assert.IsTrue(subject.Object.GetIsPasswordExpired(30));

                subject.Object.PasswordChanged = now.AddDays(-30).AddSeconds(-1);
                Assert.IsTrue(subject.Object.GetIsPasswordExpired(30));

                subject.Object.PasswordChanged = now.AddDays(-40);
                Assert.IsTrue(subject.Object.GetIsPasswordExpired(30));
            }
        }

        [TestClass]
        public class SendAccountNameReminder
        {
            //[TestMethod]
            //public void RaisesUsernameReminderRequestedEvent()
            //{
            //    var ua = new UserAccount();
                
            //    ua.SendAccountNameReminder();
                
            //    IEventSource es = ua as IEventSource;
            //    Assert.IsTrue(es.Events.Any(x => x is UsernameReminderRequestedEvent));
            //}
        }

        [TestClass]
        public class ChangeUsername
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NewUsernameIsNull_Throws()
            {
                var sub = new UserAccount();
                
                sub.ChangeUsername(null);
            }

            //[TestMethod]
            //public void RaisesUsernameChangedEvent()
            //{
            //    var ua = new UserAccount();

            //    ua.ChangeUsername("foo");

            //    IEventSource es = ua as IEventSource;
            //    Assert.IsTrue(es.Events.Any(x => x is UsernameChangedEvent));
            //}
            
            [TestMethod]
            public void NameIsAssigned()
            {
                var ua = new UserAccount();
                ua.Username = "bar";

                ua.ChangeUsername("foo");

                Assert.AreEqual("foo", ua.Username);
            }


        }
    }
}
