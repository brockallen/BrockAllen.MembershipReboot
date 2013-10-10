using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.AccountService
{
    [TestClass]
    public class UserAccountValidationTests
    {
        [TestMethod]
        public void UsernameDoesNotContainAtSign_NoAtSign_Succeeds()
        {
            var result = UserAccountValidation.UsernameDoesNotContainAtSign.Validate(new MockUserAccountService().Object, new MockUserAccount().Object, "test");
            Assert.IsNull(result);
        }
        [TestMethod]
        public void UsernameDoesNotContainAtSign_AtSign_Fails()
        {
            var result = UserAccountValidation.UsernameDoesNotContainAtSign.Validate(new MockUserAccountService().Object, new MockUserAccount().Object, "test@test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UsernameMustNotAlreadyExist_UsernameExistsInService_Fails()
        {
            var mockSvc = new MockUserAccountService();
            var acct1 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            mockSvc.MockUserAccounts(acct1);

            var acct2 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u2@t2.com");
            var result = UserAccountValidation.UsernameMustNotAlreadyExist.Validate(mockSvc.Object, acct2.Object, acct2.Object.Username);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void UsernameMustNotAlreadyExist_UsernameDoesNotExistsInService_Succeeds()
        {
            var mockSvc = new MockUserAccountService();
            var acct1 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            mockSvc.MockUserAccounts(acct1);

            var acct2 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u2", "p1", "u1@t1.com");
            var result = UserAccountValidation.UsernameMustNotAlreadyExist.Validate(mockSvc.Object, acct2.Object, acct2.Object.Username);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void EmailIsValidFormat_ValidEmail_Succeeds()
        {
            var mockSvc = new MockUserAccountService();
            var acct = new MockUserAccount();
            var result = UserAccountValidation.EmailIsValidFormat.Validate(mockSvc.Object, acct.Object, "test@foo.com");
            Assert.IsNull(result);
        }
        [TestMethod]
        public void EmailIsValidFormat_InValidEmail_Fails()
        {
            var mockSvc = new MockUserAccountService();
            var acct = new MockUserAccount();
            var result = UserAccountValidation.EmailIsValidFormat.Validate(mockSvc.Object, acct.Object, "test");
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EmailMustNotAlreadyExist_EmailExistsInService_Fails()
        {
            var mockSvc = new MockUserAccountService();
            var acct1 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            mockSvc.MockUserAccounts(acct1);

            var acct2 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            var result = UserAccountValidation.EmailMustNotAlreadyExist.Validate(mockSvc.Object, acct2.Object, acct2.Object.Email);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EmailMustNotAlreadyExist_EmailDoesNotExistsInService_Succeeds()
        {
            var mockSvc = new MockUserAccountService();
            var acct1 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            mockSvc.MockUserAccounts(acct1);

            var acct2 = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u2@t2.com");
            var result = UserAccountValidation.EmailMustNotAlreadyExist.Validate(mockSvc.Object, acct2.Object, acct2.Object.Email);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void PasswordMustBeDifferentThanCurrent_SamePassword_Fails()
        {
            var mockSvc = new MockUserAccountService();
            var acct = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            acct.Object.LastLogin = DateTime.Now;
            mockSvc.MockUserAccounts(acct);

            var result = UserAccountValidation.PasswordMustBeDifferentThanCurrent.Validate(mockSvc.Object, acct.Object, "p1");
            Assert.IsNotNull(result);
        }
        [TestMethod]
        public void PasswordMustBeDifferentThanCurrent_DifferentPassword_Succeeds()
        {
            var mockSvc = new MockUserAccountService();
            var acct = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            acct.Object.LastLogin = DateTime.Now;
            mockSvc.MockUserAccounts(acct);

            var result = UserAccountValidation.PasswordMustBeDifferentThanCurrent.Validate(mockSvc.Object, acct.Object, "p2");
            Assert.IsNull(result);
        }
        [TestMethod]
        public void PasswordMustBeDifferentThanCurrent_NewAccount_Succeeds()
        {
            var mockSvc = new MockUserAccountService();
            var acct = new MockUserAccount(mockSvc.SecuritySettings.DefaultTenant, "u1", "p1", "u1@t1.com");
            acct.Object.LastLogin = null;
            mockSvc.MockUserAccounts(acct);

            var result = UserAccountValidation.PasswordMustBeDifferentThanCurrent.Validate(mockSvc.Object, acct.Object, "p1");
            Assert.IsNull(result);
        }
    }
}
