/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

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
        MembershipRebootConfiguration configuration;
        public string LastVerificationKey { get; set; }
        public string LastMobileCode { get; set; }

        int oldIterations;
        [TestInitialize]
        public void Init()
        {
            oldIterations = SecuritySettings.Instance.PasswordHashingIterationCount;
            SecuritySettings.Instance.PasswordHashingIterationCount = 1; // tests will run faster

            configuration = new MembershipRebootConfiguration();
            configuration.AddEventHandler(new KeyNotification(this));
            repository = new FakeUserAccountRepository(); 
            subject = new UserAccountService(configuration, repository);
        }

        class KeyNotification : 
            IEventHandler<AccountCreatedEvent>, 
            IEventHandler<PasswordResetRequestedEvent>,
            IEventHandler<EmailChangeRequestedEvent>,
            IEventHandler<MobilePhoneChangeRequestedEvent>,
            IEventHandler<TwoFactorAuthenticationCodeNotificationEvent>
        {
            UserAccountServiceTests instance;
            public KeyNotification (UserAccountServiceTests instance)
            {
                this.instance = instance;
            }

            public void Handle(AccountCreatedEvent evt)
            {
                instance.LastVerificationKey = evt.VerificationKey;
            }

            public void Handle(PasswordResetRequestedEvent evt)
            {
                instance.LastVerificationKey = evt.VerificationKey;
            }

            public void Handle(EmailChangeRequestedEvent evt)
            {
                instance.LastVerificationKey = evt.VerificationKey;
            }

            public void Handle(MobilePhoneChangeRequestedEvent evt)
            {
                instance.LastMobileCode = evt.Code;
            }

            public void Handle(TwoFactorAuthenticationCodeNotificationEvent evt)
            {
                instance.LastMobileCode = evt.Code;
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = oldIterations;
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
        public void GetByUsername_MultiTenant_UsernamesNotUniqueAcrossTenants_NoTenant_ReturnsNull()
        {
            configuration.UsernamesUniqueAcrossTenants = false;
            configuration.MultiTenant = true;
            subject.CreateAccount("tenant", "user", "pass", "user@test.com");

            var acct = subject.GetByUsername("user");
            Assert.IsNull(acct);
        }
        [TestMethod]
        public void GetByUsername_MultiTenant_UsernamesUniqueAcrossTenants_NoTenant_ReturnsCorrectAccount()
        {
            configuration.UsernamesUniqueAcrossTenants = true;
            configuration.MultiTenant = true;
            subject.CreateAccount("tenant", "user", "pass", "user@test.com");

            var acct = subject.GetByUsername("user");
            Assert.IsNotNull(acct);
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
            configuration.RequireAccountVerification = true;
            configuration.AllowLoginAfterAccountCreation = true;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void CreateAccount_SettingsDoesNotRequiresVerification_CanLogin()
        {
            configuration.RequireAccountVerification = false;
            configuration.AllowLoginAfterAccountCreation = true;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void CreateAccount_SettingsDoNotAllowLoginAfterCreate_CannotLogin()
        {
            configuration.RequireAccountVerification = false;
            configuration.AllowLoginAfterAccountCreation = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void CreateAccount_AllowLoginAfterCreate_CanLogin()
        {
            configuration.RequireAccountVerification = false;
            configuration.AllowLoginAfterAccountCreation = true;
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
            configuration.MultiTenant = true;
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
            configuration.MultiTenant = false;
            configuration.DefaultTenant = "foo";
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
            configuration.EmailIsUsername = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            var acct = repository.Get(id);
            Assert.AreEqual("test@test.com", acct.Username);
        }

        [TestMethod]
        public void VerifiyAccount_AllowsUserToLogin()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyAccount(LastVerificationKey, "pass");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void VerifiyAccount_InvalidPassword_DoesNotAllowUserToLogin()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyAccount(LastVerificationKey, "bad value");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void VerifiyAccount_InvalidPassword_ReturnsFalse()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.VerifyAccount(LastVerificationKey, "bad value"));
        }

        [TestMethod]
        public void VerifiyAccount_ValidKey_ReturnsTrue()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.VerifyAccount(LastVerificationKey, "pass"));
        }
        
        [TestMethod]
        public void VerifiyAccount_InvalidKey_ReturnsFalse()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsFalse(subject.VerifyAccount("123", "pass"));
        }

        [TestMethod]
        public void CancelNewAccount_CloseAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.CancelNewAccount(LastVerificationKey);
            Assert.IsTrue(acct.IsAccountClosed);
        }
        
        [TestMethod]
        public void CancelNewAccount_DeletesAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsNotNull(repository.Get(acct.ID));
            subject.CancelNewAccount(LastVerificationKey);
            Assert.IsNull(repository.Get(acct.ID));
        }
        
        [TestMethod]
        public void CancelNewAccount_ValidKey_ReturnsTrue()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.CancelNewAccount(LastVerificationKey));
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
            subject.VerifyAccount(LastVerificationKey, "pass");
            subject.ChangeEmailRequest(acct.ID, "test2@test.com");

            Assert.IsFalse(subject.CancelNewAccount(LastVerificationKey));
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
            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void Authenticate_InvalidCredentials_ReturnsFalse()
        {
            configuration.RequireAccountVerification = false;
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
            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct2;
            subject.Authenticate("test", "pass", out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithEmail_ValidEmail_ReturnsTrue()
        {
            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.AuthenticateWithEmail("test@test.com", "pass"));
        }

        [TestMethod]
        public void AuthenticateWithEmail_ReturnsCorrectAccount()
        {
            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct2;
            subject.AuthenticateWithEmail("test@test.com", "pass", out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithEmail_InvalidCredentials_ReturnsFalse()
        {
            configuration.RequireAccountVerification = false;
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
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct;
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct));
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct));
        }

        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_SecuritySettingsEmailIsUsername_ReturnsCorrectValue()
        {
            configuration.EmailIsUsername = true;
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct;
            Assert.IsFalse(subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct));
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct));
        }
        
        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_ReturnsCorrectAccount()
        {
            configuration.RequireAccountVerification = false;
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
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            subject.ChangeMobilePhoneFromCode(id, LastMobileCode);
            subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Mobile);

            subject.Authenticate("test", "pass");

            Assert.IsTrue(subject.AuthenticateWithCode(id, LastMobileCode));
        }
        
        [TestMethod]
        public void AuthenticateWithCode_ValidCode_ReturnsCorrectAccount()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, LastMobileCode);
            subject.ConfigureTwoFactorAuthentication(acct.ID, TwoFactorAuthMode.Mobile);

            subject.Authenticate("test", "pass");

            acct = subject.GetByID(id);
            UserAccount acct2;
            subject.AuthenticateWithCode(id, LastMobileCode, out acct2);
            Assert.AreEqual(acct.ID, acct2.ID);
        }

        [TestMethod]
        public void AuthenticateWithCode_InvalidCode_ReturnsFalse()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            subject.ChangeMobilePhoneFromCode(id, LastMobileCode);
            subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Mobile);

            subject.Authenticate("test", "pass");

            Assert.IsFalse(subject.AuthenticateWithCode(id, LastMobileCode + "123"));
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

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsTrue(subject.AuthenticateWithCertificate(cert));
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndValidAccountID_ReturnsTrue()
        {
            var cert = GetTestCert();

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsTrue(subject.AuthenticateWithCertificate(acct.ID, cert));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_ValidCert_ReturnsCorrectAccount()
        {
            var cert = GetTestCert();

            configuration.RequireAccountVerification = false;
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

            configuration.RequireAccountVerification = false;
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

            configuration.RequireAccountVerification = false;
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

            configuration.RequireAccountVerification = false;
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

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            acct.AddCertificate(cert);
            subject.Update(acct);

            Assert.IsFalse(subject.AuthenticateWithCertificate(acct.ID, cert2));
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndInvalidAccountId_Throws()
        {
            var cert = GetTestCert();

            configuration.RequireAccountVerification = false;
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
        public void Authenticate_MultiTenancy_UsernamesUniqueAcrossTenants_NoTenantAPI_ShouldFindUser()
        {
            configuration.RequireAccountVerification = false;
            configuration.MultiTenant = true;
            configuration.UsernamesUniqueAcrossTenants = true;
            
            var acct = subject.CreateAccount("tenant", "user", "pass", "user@test.com");

            var result = subject.Authenticate("user", "pass");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ConfigureTwoFactorAuthentication_Mobile_UpdatesRepository()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var code = LastMobileCode;
            subject.ChangeMobilePhoneFromCode(id, code);

            subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Mobile);

            var acct = repository.Get(id);
            Assert.AreEqual(TwoFactorAuthMode.Mobile, acct.AccountTwoFactorAuthMode);
        }

        [TestMethod]
        public void ConfigureTwoFactorAuthentication_Mobile_RequiresMobilePhoneNumber()
        {
            configuration.RequireAccountVerification = false;
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
            configuration.RequireAccountVerification = false;
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
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangePassword(id, "pass", "pass2");

            Assert.IsFalse(subject.Authenticate("test", "pass"));
            Assert.IsTrue(subject.Authenticate("test", "pass2"));
        }
        
        [TestMethod]
        public void ChangePassword_InvalidAccountId_Throws()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = LastVerificationKey;

            subject.ChangePasswordFromResetKey(key, "pass2");

            Assert.IsFalse(subject.Authenticate("test", "pass"));
            Assert.IsTrue(subject.Authenticate("test", "pass2"));
        }
        
        [TestMethod]
        public void ResetPassword_UnverifiedAccount_CannotChangePassword()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = LastVerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey(key, "pass2"));
            Assert.IsFalse(subject.Authenticate("test", "pass2"));
        }

        [TestMethod]
        public void ResetPassword_EmptyEmail_FailsValidation()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            var key = LastVerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey(key, "pass2"));
        }

        [TestMethod]
        public void ChangePasswordFromResetKey_EmptyKey_ReturnsFalse()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = LastVerificationKey;

            Assert.IsFalse(subject.ChangePasswordFromResetKey("", "pass2"));
        }

        [TestMethod]
        public void ChangePasswordFromResetKey_InvalidKey_ReturnsFalse()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = LastVerificationKey;

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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.ChangeEmailFromKey(id, "pass", LastVerificationKey);
            var acct = repository.Get(id);
            Assert.AreEqual("test2@test.com", acct.Email);
        }
        
        [TestMethod]
        public void ChangeEmailRequest_NonuniqueEmail_FailsValidation()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            var acct2 = subject.CreateAccount("test2", "pass", "test2@test.com");

            try
            {
                subject.ChangeEmailRequest(acct1.ID, "test2@test.com");
                Assert.Fail();
            }
            catch(ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "Email");
            }
        }
        [TestMethod]
        public void ChangeEmailRequest_InvalidAccountId_Throws()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            Assert.IsTrue(subject.ChangeEmailFromKey(id, "pass", LastVerificationKey));
        }

        [TestMethod]
        public void ChangeEmailFromKey_SecuritySettingsEmailIsUsername_ChangesUsername()
        {
            configuration.EmailIsUsername = true;
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.ChangeEmailFromKey(id, "pass", LastVerificationKey);

            Assert.AreEqual("test2@test.com", repository.Get(id).Username);
        }

        [TestMethod]
        public void ChangeEmailFromKey_EmptyPassword_ValidationFails()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "", LastVerificationKey);
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "pass2", LastVerificationKey);
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
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(id, "pass", "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "key");
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_InvalidAccountId_Throws()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.ChangeEmailFromKey(Guid.NewGuid(), "pass", LastVerificationKey);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void RemoveMobilePhone_RemovesMobilePhone()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, LastMobileCode);

            subject.RemoveMobilePhone(id);

            Assert.IsNull(repository.Get(id).MobilePhoneNumber);
        }

        [TestMethod]
        public void RemoveMobilePhone_InvalidAccountId_Throws()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, LastMobileCode);

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
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeMobilePhoneRequest(id, "123");
            var acct = subject.GetByID(id);
            subject.ChangeMobilePhoneFromCode(id, LastMobileCode);

            Assert.AreEqual("123", repository.Get(id).MobilePhoneNumber);
        }

        [TestMethod]
        public void ChangeMobilePhoneRequest_EmptyMobilePhone_FailsValidation()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            try
            {
                subject.ChangeMobilePhoneRequest(id, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidPhone, ex.Message);
            }
        }

        [TestMethod]
        public void ChangeMobilePhoneRequest_NonUniqueMobilePhone_FailsValidation()
        {
            configuration.RequireAccountVerification = false;
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            subject.ChangeMobilePhoneRequest(acct1.ID, "123");
            subject.ChangeMobilePhoneFromCode(acct1.ID, LastMobileCode);

            var acct2 = subject.CreateAccount("test2", "pass", "test2@test.com");

            try
            {
                subject.ChangeMobilePhoneRequest(acct2.ID, "123");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "phone");
            }
        }

        [TestMethod]
        public void ChangeMobilePhoneFromCode_NonUniqueMobilePhone_FailsValidation()
        {
            configuration.RequireAccountVerification = false;
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            var acct2 = subject.CreateAccount("test2", "pass", "test2@test.com");
            
            subject.ChangeMobilePhoneRequest(acct1.ID, "123");
            var acct1Code = LastMobileCode;
            subject.ChangeMobilePhoneRequest(acct2.ID, "123");
            var acct2Code = LastMobileCode;

            subject.ChangeMobilePhoneFromCode(acct1.ID, acct1Code);
            try
            {
                subject.ChangeMobilePhoneFromCode(acct2.ID, acct2Code);
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                StringAssert.Contains(ex.Message, "phone");
            }
        }

        [TestMethod]
        public void ChangeMobilePhoneRequest_InvalidAccountId_Throws()
        {
            configuration.RequireAccountVerification = false;
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
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ChangeMobilePhoneRequest(id, "123");
            Assert.IsTrue(subject.ChangeMobilePhoneFromCode(id, LastMobileCode));
        }

        [TestMethod]
        public void ChangeMobilePhoneFromCode_EmptyCode_ReturnsFalse()
        {
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeMobilePhoneRequest(id, "123");

            Assert.IsFalse(subject.ChangeMobilePhoneFromCode(id, ""));
        }




    }
}
