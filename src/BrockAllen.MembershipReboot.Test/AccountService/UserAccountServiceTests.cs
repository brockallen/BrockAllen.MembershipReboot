using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Reflection;
using System.IO;

namespace BrockAllen.MembershipReboot.Test.AccountService
{
    [TestClass]
    public class UserAccountServiceTests
    {
        UserAccountService subject;
        FakeUserAccountRepository repository;
        SecuritySettings securitySettings;
        MembershipRebootConfiguration configuration;

        [TestInitialize]
        public void Init()
        {
            securitySettings = new SecuritySettings();
            securitySettings.PasswordHashingIterationCount = 1; // tests will run faster
            configuration = new MembershipRebootConfiguration(securitySettings);
            repository = new FakeUserAccountRepository(); 
            subject = new UserAccountService(configuration, repository);
        }

        [TestMethod]
        public void ctor_NullConfig_Throws()
        {
            try
            {
                new UserAccountService(null, repository);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                StringAssert.Contains(ex.Message, "configuration");
            }
        }
        
        [TestMethod]
        public void ctor_NullRepository_Throws()
        {
            try
            {
                new UserAccountService(configuration, null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                StringAssert.Contains(ex.Message, "userRepository");
            } 
            
            try
            {
                new UserAccountService(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                StringAssert.Contains(ex.Message, "userRepository");
            }
        }

        [TestMethod]
        public void Update_NullUserAccount_Throws()
        {
            try
            {
                subject.Update(null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                StringAssert.Contains(ex.Message, "account");
            }
        }

        [TestMethod]
        public void GetByVerificationKey_EmptyKey_ReturnsNull()
        {
            Assert.IsNull(subject.GetByVerificationKey(""));
        }

        [TestMethod]
        public void GetByCertificate_EmptyTenant_ReturnsNull()
        {
            Assert.IsNull(subject.GetByCertificate("", "123"));
        }

        [TestMethod]
        public void GetByCertificate_EmptyThumbprint_ReturnsNull()
        {
            Assert.IsNull(subject.GetByCertificate("tenant", ""));
        }

        [TestMethod]
        public void UsernameExists_EmptyTenant_ReturnsFalse()
        {
            Assert.IsFalse(subject.UsernameExists("", "test"));
        }
        [TestMethod]
        public void UsernameExists_EmptyUsername_ReturnsFalse()
        {
            Assert.IsFalse(subject.UsernameExists("tenant", ""));
        }

        [TestMethod]
        public void EmailExists_EmptyTenant_ReturnsFalse()
        {
            Assert.IsFalse(subject.EmailExists("", "test@test.com"));
        }
        
        [TestMethod]
        public void EmailExists_EmptyEmail_ReturnsFalse()
        {
            Assert.IsFalse(subject.EmailExists("tenant", ""));
        }

        [TestMethod]
        public void CreateAccount_CreatesAccountInRepository()
        {
            var result = subject.CreateAccount("test", "test", "test@test.com");
            Assert.AreSame(repository.Get(result.ID), result);
        }

        [TestMethod]
        public void CreateAccount_SettingsRequiresVerification_CannotLogin()
        {
            securitySettings.RequireAccountVerification = true;
            securitySettings.AllowLoginAfterAccountCreation = true;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void CreateAccount_SettingsDoesNotRequiresVerification_CanLogin()
        {
            securitySettings.RequireAccountVerification = false;
            securitySettings.AllowLoginAfterAccountCreation = true;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void CreateAccount_SettingsDoNotAllowLoginAfterCreate_CannotLogin()
        {
            securitySettings.RequireAccountVerification = false;
            securitySettings.AllowLoginAfterAccountCreation = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void CreateAccount_AllowLoginAfterCreate_CanLogin()
        {
            securitySettings.RequireAccountVerification = false;
            securitySettings.AllowLoginAfterAccountCreation = true;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void CreateAccount_EmptyUsername_FailsValidation()
        {
            try
            {
                subject.CreateAccount("", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Username");
            }
        }
        
        [TestMethod]
        public void CreateAccount_AtSignInUsername_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test@test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Username");
            }
        }
        
        [TestMethod]
        public void CreateAccount_DuplicateUsername_FailsValidation()
        {
            subject.CreateAccount("test", "pass", "test@test.com");

            try
            {
                subject.CreateAccount("test", "pass2", "test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Username");
            }
        }
        
        [TestMethod]
        public void CreateAccount_EmptyEmail_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test", "pass", "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Email");
            }
        }
        
        [TestMethod]
        public void CreateAccount_DuplicateEmail_FailsValidation()
        {
            subject.CreateAccount("test", "pass", "test@test.com");

            try
            {
                subject.CreateAccount("test2", "pass2", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Email");
            }
        }

        [TestMethod]
        public void CreateAccount_SettingsRequireMultiTenant_EmptyTenant_Throws()
        {
            securitySettings.MultiTenant = true;
            try
            {
                subject.CreateAccount("", "test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                StringAssert.Contains(ex.Message, "tenant");
            }
        }
        
        [TestMethod]
        public void CreateAccount_SettingsSingleMultiTenant_UserAccountUsesDefaultTenant()
        {
            securitySettings.MultiTenant = false;
            securitySettings.DefaultTenant = "foo";
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            var acct = repository.Get(id);
            Assert.AreEqual("foo", acct.Tenant);
        }

        [TestMethod]
        public void CreateAccount_EmptyPassword_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test", "", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Password");
            }
        }
        
        [TestMethod]
        public void CreateAccount_SettingsRequireEmailIsUsername_UsernameIsEmail()
        {
            securitySettings.EmailIsUsername = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            var acct = repository.Get(id);
            Assert.AreEqual("test@test.com", acct.Username);
        }

        [TestMethod]
        public void VerifiyAccount_AllowsUserToLogin()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyAccount(acct.VerificationKey);
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void VerifiyAccount_ValidKey_ReturnsTrue()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.VerifyAccount(acct.VerificationKey));
        }
        
        [TestMethod]
        public void VerifiyAccount_InvalidKey_ReturnsFalse()
        {
            Assert.IsFalse(subject.VerifyAccount("123"));
        }

        [TestMethod]
        public void CancelNewAccount_CloseAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.CancelNewAccount(acct.VerificationKey);
            Assert.IsTrue(acct.IsAccountClosed);
        }
        
        [TestMethod]
        public void CancelNewAccount_DeletesAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsNotNull(repository.Get(acct.ID));
            subject.CancelNewAccount(acct.VerificationKey);
            Assert.IsNull(repository.Get(acct.ID));
        }
        
        [TestMethod]
        public void CancelNewAccount_ValidKey_ReturnsTrue()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.CancelNewAccount(acct.VerificationKey));
        }
        
        [TestMethod]
        public void CancelNewAccount_InvalidKey_ReturnsFalse()
        {
            Assert.IsFalse(subject.CancelNewAccount("123"));
        }

        [TestMethod]
        public void CancelNewAccount_KeyUsedForAnotherPurpose_ReturnsFalse()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyAccount(acct.VerificationKey);
            subject.ChangeEmailRequest(acct.ID, "test2@test.com");

            Assert.IsFalse(subject.CancelNewAccount(repository.Get(acct.ID).VerificationKey));
        }

        [TestMethod]
        public void DeleteAccount_DeletesAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsNotNull(repository.Get(acct.ID));
            subject.DeleteAccount(acct.ID);
            Assert.IsNull(repository.Get(acct.ID));
        }
        
        [TestMethod]
        public void DeleteAccount_InvalidAccountID_Throws()
        {
            try
            {
                subject.DeleteAccount(Guid.NewGuid());
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void Authenticate_ValidCredentials_ReturnsTrue()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void Authenticate_InvalidCredentials_ReturnsFalse()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.Authenticate("test", "abc"));
            Assert.IsFalse(subject.Authenticate("test", "123"));
            Assert.IsFalse(subject.Authenticate("test", ""));
            Assert.IsFalse(subject.Authenticate("test", null));
            Assert.IsFalse(subject.Authenticate("", "pass"));
            Assert.IsFalse(subject.Authenticate(null, "pass"));
            Assert.IsFalse(subject.Authenticate("test2", "pass"));
        }
        
        [TestMethod]
        public void Authenticate_ReturnsCorrectAccount()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct2;
            subject.Authenticate("test", "pass", out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithEmail_ValidEmail_ReturnsTrue()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.AuthenticateWithEmail("test@test.com", "pass"));
        }

        [TestMethod]
        public void AuthenticateWithEmail_ReturnsCorrectAccount()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct2;
            subject.AuthenticateWithEmail("test@test.com", "pass", out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithEmail_InvalidCredentials_ReturnsFalse()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.Authenticate("test@test.com", "abc"));
            Assert.IsFalse(subject.Authenticate("test@test.com", "123"));
            Assert.IsFalse(subject.Authenticate("test@test.com", ""));
            Assert.IsFalse(subject.Authenticate("test@test.com", null));
            Assert.IsFalse(subject.Authenticate("", "pass"));
            Assert.IsFalse(subject.Authenticate(null, "pass"));
            Assert.IsFalse(subject.Authenticate("test2", "pass"));
        }

        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_ValidCredentials_ReturnsTrue()
        {
            securitySettings.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct;
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct));
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct));
        }

        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_SecuritySettingsEmailIsUsername_ReturnsCorrectValue()
        {
            securitySettings.EmailIsUsername = true;
            securitySettings.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct;
            Assert.IsFalse(subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct));
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct));
        }
        
        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_ReturnsCorrectAccount()
        {
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            
            UserAccount acct2;
            subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
            
            subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithCode_ValidCode_ReturnsTrue()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, acct.MobileCode);
            subject.ConfigureTwoFactorAuthentication(acct.ID, TwoFactorAuthMode.Mobile);

            subject.Authenticate("test", "pass");

            acct = subject.GetByID(id);
            Assert.IsTrue(subject.AuthenticateWithCode(id, acct.MobileCode));
        }
        
        [TestMethod]
        public void AuthenticateWithCode_ValidCode_ReturnsCorrectAccount()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, acct.MobileCode);
            subject.ConfigureTwoFactorAuthentication(acct.ID, TwoFactorAuthMode.Mobile);

            subject.Authenticate("test", "pass");

            acct = subject.GetByID(id);
            UserAccount acct2;
            subject.AuthenticateWithCode(id, acct.MobileCode, out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithCode_InvalidCode_ReturnsFalse()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, acct.MobileCode);
            subject.ConfigureTwoFactorAuthentication(acct.ID, TwoFactorAuthMode.Mobile);

            subject.Authenticate("test", "pass");

            acct = subject.GetByID(id);
            Assert.IsFalse(subject.AuthenticateWithCode(id, acct.MobileCode + "123"));
        }
        
        [TestMethod]
        public void AuthenticateWithCode_InvalidAccountId_Throws()
        {
            try
            {
                subject.AuthenticateWithCode(Guid.NewGuid(), "123");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        static X509Certificate2 GetTestCert()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("BrockAllen.MembershipReboot.Test.test.cer"))
            {
                var ms = new MemoryStream();
                s.CopyTo(ms);
                return new X509Certificate2(ms.ToArray());
            }
        }
        static X509Certificate2 GetTestCert2()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("BrockAllen.MembershipReboot.Test.test2.cer"))
            {
                var ms = new MemoryStream();
                s.CopyTo(ms);
                return new X509Certificate2(ms.ToArray());
            }
        }

        [TestMethod]
        public void AuthenticateWithCertificate_ValidCert_ReturnsTrue()
        {
            var cert = GetTestCert();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsTrue(subject.AuthenticateWithCertificate(cert));
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndValidAccountID_ReturnsTrue()
        {
            var cert = GetTestCert();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsTrue(subject.AuthenticateWithCertificate(acct.ID, cert));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_ValidCert_ReturnsCorrectAccount()
        {
            var cert = GetTestCert();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            UserAccount acct2;
            subject.AuthenticateWithCertificate(cert, out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndValidAccountId_ReturnsCorrectAccount()
        {
            var cert = GetTestCert();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            UserAccount acct2;
            subject.AuthenticateWithCertificate(acct.ID, cert, out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithCertificate_WrongCert_ReturnsFalse()
        {
            var cert = GetTestCert();
            var cert2 = GetTestCert2();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsFalse(subject.AuthenticateWithCertificate(cert2));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_InvalidCert_ReturnsFalse()
        {
            var cert = GetTestCert();
            var cert2 = new X509Certificate2();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsFalse(subject.AuthenticateWithCertificate(cert2));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_WrongCertAndValidAccountId_ReturnsFalse()
        {
            var cert = GetTestCert();
            var cert2 = GetTestCert2();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsFalse(subject.AuthenticateWithCertificate(acct.ID, cert2));
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndInvalidAccountId_Throws()
        {
            var cert = GetTestCert();

            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            try
            {
                subject.AuthenticateWithCertificate(Guid.NewGuid(), cert);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void ConfigureTwoFactorAuthentication_Mobile_UpdatesRepository()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var code = subject.GetByID(id).MobileCode;
            subject.ChangeMobilePhoneFromCode(id, code);

            subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Mobile);

            var acct = repository.Get(id);
            Assert.AreEqual(TwoFactorAuthMode.Mobile, acct.AccountTwoFactorAuthMode);
        }

        [TestMethod]
        public void ConfigureTwoFactorAuthentication_Mobile_RequiresMobilePhoneNumber()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Mobile);
                Assert.Fail();
            }
            catch (ValidationException)
            {
            }
        }

        [TestMethod]
        public void ConfigureTwoFactorAuthentication_Certificate_UpdatesRepository()
        {
            var cert = GetTestCert();
            securitySettings.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);
            var id = acct.ID;

            subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Certificate);

            acct = repository.Get(id);
            Assert.AreEqual(TwoFactorAuthMode.Certificate, acct.AccountTwoFactorAuthMode);
        }

        [TestMethod]
        public void ConfigureTwoFactorAuthentication_Certificate_RequiresCertificate()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Certificate);
                Assert.Fail();
            }
            catch (ValidationException)
            {
            }
        }

        [TestMethod]
        public void SetPassword_ChangesPassword()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.SetPassword(id, "pass2");

            Assert.IsFalse(subject.Authenticate("test", "pass"));
            Assert.IsTrue(subject.Authenticate("test", "pass2"));
        }

        [TestMethod]
        public void SetPassword_InvalidAccountId_Throws()
        {
            try
            {
                subject.SetPassword(Guid.NewGuid(), "pass2");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }
        
        [TestMethod]
        public void SetPassword_InvalidPassword_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.SetPassword(id, "");
                Assert.Fail();
            }
            catch (ValidationException)
            {
            }
        }

        [TestMethod]
        public void ChangePassword_ChangesPassword()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangePassword(id, "pass", "pass2");

            Assert.IsFalse(subject.Authenticate("test", "pass"));
            Assert.IsTrue(subject.Authenticate("test", "pass2"));
        }
        
        [TestMethod]
        public void ChangePassword_InvalidAccountId_Throws()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangePassword(Guid.NewGuid(), "pass", "pass2");
                Assert.Fail();
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void ChangePassword_EmptyOldPass_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangePassword(id, "", "pass2");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "old");
            }
        }

        [TestMethod]
        public void ChangePassword_InvalidOldPass_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangePassword(id, "abc", "pass2");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "old");
            }
        }

        [TestMethod]
        public void ChangePassword_EmptyNewPass_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangePassword(id, "pass", "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "new");
            }
        }

        [TestMethod]
        public void ResetPassword_AllowsPasswordToBeReset()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = subject.GetByID(id).VerificationKey;

            subject.ChangePasswordFromResetKey(key, "pass2");

            Assert.IsFalse(subject.Authenticate("test", "pass"));
            Assert.IsTrue(subject.Authenticate("test", "pass2"));
        }
        
        [TestMethod]
        public void ResetPassword_UnverifiedAccount_CannotChangePassword()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = subject.GetByID(id).VerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey(key, "pass2"));
            Assert.IsFalse(subject.Authenticate("test", "pass2"));
        }

        [TestMethod]
        public void ResetPassword_EmptyEmail_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ResetPassword("");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "email");
            }
        }
        
        [TestMethod]
        public void ResetPassword_InvalidEmail_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ResetPassword("test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "email");
            }
        }

        [TestMethod]
        public void ChangePasswordFromResetKey_RequiresResetPassword()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            var key = subject.GetByID(id).VerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey(key, "pass2"));
        }

        [TestMethod]
        public void ChangePasswordFromResetKey_EmptyKey_ReturnsFalse()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = subject.GetByID(id).VerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey("", "pass2"));
        }

        [TestMethod]
        public void ChangePasswordFromResetKey_InvalidKey_ReturnsFalse()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = subject.GetByID(id).VerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey("abc", "pass2"));
        }

        [TestMethod]
        public void ChangeUsername_ChangesUsername()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeUsername(id, "test2");

            Assert.AreEqual("test2", repository.Get(id).Username);
        }

        [TestMethod]
        public void ChangeUsername_UsernameExists_FailsValidation()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.CreateAccount("test2", "pass2", "test2@test.com");

            try
            {
                subject.ChangeUsername(id, "test2");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Username");
            }
        }

        [TestMethod]
        public void ChangeUsername_SecuritySettingsUsernameIsEmail_Throws()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeUsername(id, "test2");
                Assert.Fail();
            }
            catch (Exception)
            {
            }
        }
        
        [TestMethod]
        public void ChangeUsername_EmptyNewUsername_FailsValidation()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeUsername(id, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "username");
            }
        }

        [TestMethod]
        public void ChangeUsername_InvalidAccountId_Throws()
        {
            try
            {
                subject.ChangeUsername(Guid.NewGuid(), "test2");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void ChangeEmailRequest_AllowsEmailToBeChanged()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.ChangeEmailFromKey(id, "pass", repository.Get(id).VerificationKey, "test2@test.com");
            var acct = repository.Get(id);
            Assert.AreEqual("test2@test.com", acct.Email);
        }

        [TestMethod]
        public void ChangeEmailRequest_AllowsEmailWithDifferentCaseToBeChanged()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.ChangeEmailFromKey(id, "pass", repository.Get(id).VerificationKey, "TEST2@TEST.COM");
            var acct = repository.Get(id);
            Assert.AreEqual("TEST2@TEST.COM", acct.Email);
        }
        
        [TestMethod]
        public void ChangeEmailRequest_InvalidAccountId_Throws()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeEmailRequest(Guid.NewGuid(), "test2@test.com");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void ChangeEmailRequest_EmptyNewEmail_FailsValidation()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeEmailRequest(id, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "email");
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_Success_ReturnsTrue()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            Assert.IsTrue(subject.ChangeEmailFromKey(id, "pass", repository.Get(id).VerificationKey, "test2@test.com"));
        }

        [TestMethod]
        public void ChangeEmailFromKey_SecuritySettingsEmailIsUsername_ChangesUsername()
        {
            securitySettings.EmailIsUsername = true;
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.ChangeEmailFromKey(id, "pass", repository.Get(id).VerificationKey, "test2@test.com");

            Assert.AreEqual("test2@test.com", repository.Get(id).Username);
        }

        [TestMethod]
        public void ChangeEmailFromKey_EmptyPassword_ValidationFails()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "", repository.Get(id).VerificationKey, "test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "password");
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_InvalidPassword_ValidationFails()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "pass2", repository.Get(id).VerificationKey, "test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "password");
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_EmptyKey_ValidationFails()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "pass", "", "test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "key");
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_EmptyNewEmail_ValidationFails()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "pass", repository.Get(id).VerificationKey, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "email");
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_InvalidAccountId_Throws()
        {
            securitySettings.AllowLoginAfterAccountCreation = true;
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(Guid.NewGuid(), "pass", repository.Get(id).VerificationKey, "test2@test.com");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void RemoveMobilePhone_RemovesMobilePhone()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, acct.MobileCode);

            subject.RemoveMobilePhone(id);

            Assert.IsNull(repository.Get(id).MobilePhoneNumber);
        }

        [TestMethod]
        public void RemoveMobilePhone_InvalidAccountId_Throws()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, acct.MobileCode);

            try
            {
                subject.RemoveMobilePhone(Guid.NewGuid());
                Assert.Fail();
            }
            catch (ArgumentException)
            { }
        }

        [TestMethod]
        public void ChangeMobilePhoneRequest_AllowsMobileToBeChanged()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, acct.MobileCode);

            Assert.AreEqual("123", repository.Get(id).MobilePhoneNumber);
        }
        
        [TestMethod]
        public void ChangeMobilePhoneRequest_EmptyMobilePhone_FailsValidation()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeMobilePhoneRequest(id, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Phone");
            }
        }

        [TestMethod]
        public void ChangeMobilePhoneRequest_InvalidAccountId_Throws()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeMobilePhoneRequest(Guid.NewGuid(), "123");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void ChangeMobilePhoneFromCode_Success_ReturnsTrue()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            Assert.IsTrue(subject.ChangeMobilePhoneFromCode(id, acct.MobileCode));
        }

        [TestMethod]
        public void ChangeMobilePhoneFromCode_EmptyCode_ReturnsFalse()
        {
            securitySettings.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");

            Assert.IsFalse(subject.ChangeMobilePhoneFromCode(id, ""));
        }




    }
}
