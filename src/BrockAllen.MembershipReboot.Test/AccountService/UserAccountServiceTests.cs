/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Collections;
using System.Security.Claims;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Test.AccountService
{
    [TestClass]
    public class UserAccountServiceTests
    {
        TestUserAccountService subject;
        FakeUserAccountRepository repository;
        MembershipRebootConfiguration configuration;
        KeyNotification key;

        public string LastVerificationKey { get { return key.LastVerificationKey; } }
        public string LastMobileCode { get { return key.LastMobileCode; } }
        public IEvent LastEvent { get { return key.LastEvent; } }

        int oldIterations;

        [TestInitialize]
        public void Init()
        {
            oldIterations = SecuritySettings.Instance.PasswordHashingIterationCount;
            SecuritySettings.Instance.PasswordHashingIterationCount = 1; // tests will run faster

            configuration = new MembershipRebootConfiguration();
            key = new KeyNotification();
            configuration.AddEventHandler(key);
            repository = new FakeUserAccountRepository();
            subject = new TestUserAccountService(configuration, repository);
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
        public void EmailExists_WhenEmailIsNotUnique_Throws()
        {
            configuration.EmailIsUnique = false;
            try
            {
                subject.EmailExists("test@test.com");
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
        }
        
        [TestMethod]
        public void GetByEmail_WhenEmailIsNotUnique_Throws()
        {
            configuration.EmailIsUnique = false;
            try
            {
                subject.GetByEmail("test@test.com");
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
        }


        [TestMethod]
        public void CreateAccount_CreatesAccountInRepository()
        {
            var result = subject.CreateAccount("test", "test", "test@test.com");
            Assert.AreSame(repository.GetByID(result.ID), result);
        }

        [TestMethod]
        public void CreateAccount_SettingsRequiresVerification_CannotLogin()
        {
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
                Assert.AreEqual(Resources.ValidationMessages.UsernameRequired, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_EmptyEmail_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test", "pass", null);
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailRequired, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_AccountVerificationNotRequired_EmptyEmail_Succeeds()
        {
            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", null);
            Assert.IsNull(acct.Email);
            Assert.IsNull(repository.GetByID(acct.ID).Email);
        }

        [TestMethod]
        public void CreateAccount_AccountVerificationNotRequired_TwoEmptyEmails_Succeeds()
        {
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test1", "pass", null);
            subject.CreateAccount("test2", "pass", null);
        }

        [TestMethod]
        public void CreateAccount_AccountVerificationNotRequired_DuplicateEmails_FailsValidation()
        {
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test1", "pass", "test@test.com");
            try
            {
                subject.CreateAccount("test2", "pass", "test@test.com");
                Assert.Fail();
            }
            catch(ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailAlreadyInUse, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.UsernameCannotContainAtSign, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.UsernameAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_EmailIsUsername_DuplicateEmails_FailsValidation()
        {
            configuration.EmailIsUsername = true;
            subject.CreateAccount(null, "pass", "test@test.com");

            try
            {
                subject.CreateAccount(null, "pass2", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_DuplicateEmailDiffersByCase_FailsValidation()
        {
            subject.CreateAccount("test1", "pass", "test@test.com");

            try
            {
                subject.CreateAccount("test2", "pass2", "TEST@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_DuplicateUsernameDiffersByCase_FailsValidation()
        {
            subject.CreateAccount("test", "pass", "test@test.com");

            try
            {
                subject.CreateAccount("TEST", "pass2", "test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_MultiTenant_DuplicateUsernameTenantDiffersByCase_FailsValidation()
        {
            configuration.MultiTenant = true;
            subject.CreateAccount("tenant", "test", "pass", "test@test.com");

            try
            {
                subject.CreateAccount("TENANT", "test", "pass2", "test2@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_EmptyEmailWhenAccountVerificationRequired_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test", "pass", "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailRequired, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_EmptyEmailWhenAccountVerificationNotRequired_Allowed()
        {
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "");
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
                Assert.AreEqual(Resources.ValidationMessages.EmailAlreadyInUse, ex.Message);
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
            var acct = repository.GetByID(id);
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
                Assert.AreEqual(Resources.ValidationMessages.PasswordRequired, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_WhitespacePassword_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test", "   ", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.PasswordRequired, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_WhitespaceUsername_FailsValidation()
        {
            try
            {
                subject.CreateAccount(" ", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameRequired, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_RepeatSpecialCharsInUsername_FailsValidation()
        {
            try
            {
                subject.CreateAccount("test  test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCannotRepeatSpecialCharacters, ex.Message);
            }
            try
            {
                subject.CreateAccount("test--test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCannotRepeatSpecialCharacters, ex.Message);
            }
            try
            {
                subject.CreateAccount("test..test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCannotRepeatSpecialCharacters, ex.Message);
            }
            try
            {
                subject.CreateAccount("test__test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCannotRepeatSpecialCharacters, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_AllowAccountToBePassedIn()
        {
            MyUserAccount acct = new MyUserAccount();
            var result = subject.CreateAccount("tenant", "user", "pass", "user@email.com", null, null, acct);
            Assert.AreSame(acct, result);
        }

        [TestMethod]
        public void CreateAccount_AllowClaimsToBePassedIn()
        {
            MyUserAccount acct = new MyUserAccount();
            List<Claim> claims = new List<Claim> {new Claim("foo", "bar")};
            var result = subject.CreateAccount("tenant", "user", "pass", "user@email.com", null, null, acct, claims);
            Assert.AreSame(acct, result);

            var comparer = new UserClaimComparer();
            var userClaims = claims.Select(c => new UserClaim(c.Type, c.Value));
            CollectionAssert.AreEqual(
                userClaims.OrderBy(c => c, comparer).ToList(),
                result.Claims.OrderBy(c => c, comparer).ToList(),
                comparer);
        }


        [TestMethod]
        public void CreateMethod_UsernameStartsOrEndNonLetterOrDigit_FailsValidation()
        {
            try
            {
                subject.CreateAccount(" test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
            try
            {
                subject.CreateAccount("test ", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            } 
            try
            {
                subject.CreateAccount(".test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
            try
            {
                subject.CreateAccount("test.", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
            try
            {
                subject.CreateAccount("_test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
            try
            {
                subject.CreateAccount("test_", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
            try
            {
                subject.CreateAccount("test'", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
            try
            {
                subject.CreateAccount("'test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.UsernameCanOnlyStartOrEndWithLetterOrDigit, ex.Message);
            }
        }
        
        [TestMethod]
        public void CreateMethod_UsernameContainsWhitespace_Succeeds()
        {
            subject.CreateAccount("test test", "pass", "test@test.com");
        }
        [TestMethod]
        public void CreateMethod_UsernameContainsUnderscore_Succeeds()
        {
            subject.CreateAccount("test_test", "pass", "test@test.com");
        }
        [TestMethod]
        public void CreateMethod_UsernameContainsPeriods_Succeeds()
        {
            subject.CreateAccount("test.test", "pass", "test@test.com");
        }
        [TestMethod]
        public void CreateMethod_UsernameContainsDigits_Succeeds()
        {
            subject.CreateAccount("123", "pass", "test@test.com");
        }
        [TestMethod]
        public void CreateMethod_UsernameContainsSingleQuote_Succeeds()
        {
            subject.CreateAccount("te'st", "pass", "test@test.com");
        }
        [TestMethod]
        public void CreateMethod_UsernameContainsLetters_Succeeds()
        {
            subject.CreateAccount("test", "pass", "test@test.com");
        }
        
        [TestMethod]
        public void CreateMethod_EmailIsUsername_EmailContainsWhitespace_FailsValidation()
        {
            configuration.EmailIsUsername = true;

            try
            {
                subject.CreateAccount(null, "pass", "test test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidEmail, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_CanPassID_UsesID()
        {
            var id = Guid.NewGuid();
            var acct = subject.CreateAccount("user", "pass", "test@test.com", id);
            Assert.AreEqual(id, acct.ID);
        }

        [TestMethod]
        public void CreateAccount_CanPassCreatedDate_UsesCreatedDate()
        {
            var created = DateTime.UtcNow;
            var acct = subject.CreateAccount("user", "pass", "test@test.com", null, created);
            Assert.AreEqual(created, acct.Created);
        }
        [TestMethod]
        public void CreateAccount_FutureCreatedDate_Throws()
        {
            var created = DateTime.UtcNow.AddDays(1);
            try
            {
                var acct = subject.CreateAccount("user", "pass", "test@test.com", null, created);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                StringAssert.Contains(ex.Message, "dateCreated");
            }
        }

        [TestMethod]
        public void CreateAccount_SettingsRequireEmailIsUsername_UsernameIsEmail()
        {
            configuration.EmailIsUsername = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            var acct = repository.GetByID(id);
            Assert.AreEqual("test@test.com", acct.Username);
        }

        [TestMethod]
        public void CreateAccount_EmailIsNotUnique_CanCreateTwoAccountsWithSameEmail()
        {
            configuration.EmailIsUnique = false;
            var acct1 = subject.CreateAccount("test1", null, "test@test.com");
            var acct2 = subject.CreateAccount("test2", null, "test@test.com");
        }

        [TestMethod]
        public void VerifiyEmailFromKey_AllowsUserToLogin()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void VerifiyEmailFromKey_WhenEmailIsNotUnique_MustMatchCorrectAccount()
        {
            configuration.EmailIsUnique = false;
            var acct1 = subject.CreateAccount("test1", "pass", "test@test.com");
            var acct2 = subject.CreateAccount("test2", "pass", "test@test.com");
            
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            Assert.IsFalse(subject.Authenticate("test1", "pass"));
            Assert.IsTrue(subject.Authenticate("test2", "pass"));
        }

        [TestMethod]
        public void VerifiyEmailFromKey_InvalidPassword_DoesNotAllowUserToLogin()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            try
            {
                subject.VerifyEmailFromKey(LastVerificationKey, "bad value");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidPassword, ex.Message);
            }
            Assert.IsFalse(subject.Authenticate("test", "pass"));
        }
        
        [TestMethod]
        public void VerifiyAccount_InvalidPassword_ReturnsFalse()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            try
            {
                subject.VerifyEmailFromKey(LastVerificationKey, "bad value");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidPassword, ex.Message);
            } 
        }

        [TestMethod]
        public void VerifiyAccount_ValidKey_ReturnsTrue()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");

            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
        }
        
        [TestMethod]
        public void VerifiyAccount_InvalidKey_ReturnsFalse()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");

            try
            {
                subject.VerifyEmailFromKey("123", "pass");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidKey, ex.Message);
            }
        }

        [TestMethod]
        public void CancelVerification_CloseAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.CancelVerification(LastVerificationKey);
            Assert.IsTrue(acct.IsAccountClosed);
        }
        
        [TestMethod]
        public void CancelVerification_DeletesAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsNotNull(repository.GetByID(acct.ID));
            subject.CancelVerification(LastVerificationKey);
            Assert.IsNull(repository.GetByID(acct.ID));
        }
        
        [TestMethod]
        public void CancelVerification_ValidKey_ReturnsTrue()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.CancelVerification(LastVerificationKey);
        }
        
        [TestMethod]
        public void CancelVerification_InvalidKey_ReturnsFalse()
        {
            try
            {
                subject.CancelVerification("123");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidKey, ex.Message);
            }
        }

        [TestMethod]
        public void CancelVerification_KeyUsedForAnotherPurpose_Fails()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
            var key = LastVerificationKey;
            subject.ChangeEmailRequest(acct.ID, "test2@test.com");

            try
            {
                subject.CancelVerification(key);
                Assert.Fail();
            }
            catch (ValidationException)
            {
            }
        }

        [TestMethod]
        public void CancelVerification_PasswordResets_ClearsVerification()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
            subject.ResetPassword("test@test.com");
            acct = subject.GetByID(acct.ID);
            Assert.IsNotNull(acct.VerificationPurpose);
            var key = LastVerificationKey;

            subject.CancelVerification(key);
            
            acct = subject.GetByID(acct.ID);
            Assert.IsNull(acct.VerificationPurpose);
        }

        [TestMethod]
        public void CancelVerification_EmptyKey_FailsValidation()
        {
            try
            {
                subject.CancelVerification(null);
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidKey, ex.Message);
            }
        }


        [TestMethod]
        public void DeleteAccount_DeletesAccount()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsNotNull(repository.GetByID(acct.ID));
            subject.DeleteAccount(acct.ID);
            Assert.IsNull(repository.GetByID(acct.ID));
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
        public void CloseAccount_ClosesAccount()
        {
            var now = new DateTime(2012, 2, 3, 4, 5, 6);
            subject.Now = now;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsNotNull(repository.GetByID(acct.ID));
            subject.CloseAccount(acct.ID);

            acct = repository.GetByID(acct.ID);
            Assert.IsTrue(acct.IsAccountClosed);
            Assert.IsNotNull(acct.AccountClosed);
            Assert.AreEqual(now, acct.AccountClosed.Value);
            Assert.IsTrue(repository.UpdateWasCalled);
        }

        [TestMethod]
        public void CloseAccount_InvalidAccountID_Throws()
        {
            try
            {
                subject.CloseAccount(Guid.NewGuid());
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
            AuthenticationFailureCode failureCode;
            Assert.IsTrue(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.None, failureCode);
        }
        

        [TestMethod]
        public void Authenticate_InvalidCredentials_ReturnsFalse()
        {
            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "abc", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.Authenticate("test", "123", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.Authenticate("test", "", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.Authenticate("test", null, out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.Authenticate("", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.Authenticate((string) null, "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.Authenticate("test2", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
        }

        [TestMethod]
        public void Authenticate_TooManyBadPasswords_Fails()
        {
            this.configuration.RequireAccountVerification = false;
            this.configuration.AccountLockoutFailedLoginAttempts = 5;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.FailedLoginAttemptsExceeded, failureCode);
        }

        [TestMethod]
        public void Authenticate_AccountLocked_AfterLockoutDuration_LoginAllowed()
        {
            this.configuration.RequireAccountVerification = false;
            this.configuration.AccountLockoutFailedLoginAttempts = 5;
            this.configuration.AccountLockoutDuration = TimeSpan.FromMinutes(1);

            subject.Now = new DateTime(2014, 3, 18, 9, 0, 0);

            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
            subject.Now += new TimeSpan(0, 1, 1);
            Assert.IsTrue(subject.Authenticate("test", "pass"));
        }

        [TestMethod]
        public void Authenticate_AccountLocked_AfterLockoutDuration_FailedAttemptsReset()
        {
            this.configuration.RequireAccountVerification = false;
            this.configuration.AccountLockoutFailedLoginAttempts = 5;
            this.configuration.AccountLockoutDuration = TimeSpan.FromMinutes(1);

            subject.Now = new DateTime(2014, 3, 18, 9, 0, 0);

            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
            subject.Now += new TimeSpan(0, 1, 1);
            subject.Authenticate("test", "bad");
            acct = subject.GetByID(acct.ID);
            Assert.AreEqual(1, acct.FailedLoginCount);
        }

        [TestMethod]
        public void Authenticate_AccountNotVerified_Fails()
        {
            this.configuration.RequireAccountVerification = true;

            var acc = subject.CreateAccount("test", "pass", "test@test.com");
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.AccountNotVerified, failureCode);
        }

        [TestMethod]
        public void Authenticate_LoginNotAllowed_Fails()
        {
            this.configuration.RequireAccountVerification = false;

            var acc = subject.CreateAccount("test", "pass", "test@test.com");
            acc.IsLoginAllowed = false;
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.LoginNotAllowed, failureCode);
        }

        [TestMethod]
        public void Authenticate_AccountClosed_Fails()
        {
            this.configuration.RequireAccountVerification = false;

            var acc = subject.CreateAccount("test", "pass", "test@test.com");
            acc.IsAccountClosed = true;
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.AccountClosed, failureCode);
        }

        [TestMethod]
        public void Authenticate_AccountMissingPassword_Fails()
        {
            this.configuration.RequireAccountVerification = false;

            string nullPassword = null;
// ReSharper disable ExpressionIsAlwaysNull
            subject.CreateAccount("test", nullPassword, "test@test.com");
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", nullPassword, out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
// ReSharper restore ExpressionIsAlwaysNull
        }

        [TestMethod]
        public void Authenticate_MobileTwoFactorAuthRequired_MissingMobileNumber_Fails()
        {
            this.configuration.RequireAccountVerification = false;

            var acc = subject.CreateAccount("test", "pass", "test@test.com");
            acc.MobilePhoneNumber = "";
            acc.AccountTwoFactorAuthMode = TwoFactorAuthMode.Mobile;

            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.AccountNotConfiguredWithMobilePhone, failureCode);
        }

        [TestMethod]
        public void Authenticate_CertificateTwoFactorAuthRequired_NoConfiguredCerts_Fails()
        {
            this.configuration.RequireAccountVerification = false;

            var acc = subject.CreateAccount("test", "pass", "test@test.com");
            acc.AccountTwoFactorAuthMode = TwoFactorAuthMode.Certificate;

            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.Authenticate("test", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.AccountNotConfiguredWithCertificates, failureCode);
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
            AuthenticationFailureCode failureCode;
            Assert.IsTrue(subject.AuthenticateWithEmail("test@test.com", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.None, failureCode);
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
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.AuthenticateWithEmail("test@test.com", "abc", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.AuthenticateWithEmail("test@test.com", "123", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.AuthenticateWithEmail("test@test.com", "", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.AuthenticateWithEmail("test@test.com", null, out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.AuthenticateWithEmail("", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.AuthenticateWithEmail(null, "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
            Assert.IsFalse(subject.AuthenticateWithEmail("test2", "pass", out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.InvalidCredentials, failureCode);
        }

        [TestMethod]
        public void AuthenticateWithEmail_EmailIsNotUnique_Throws()
        {
            configuration.EmailIsUnique = false;
            try
            {
                subject.AuthenticateWithEmail("test@test.com", "pass");
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_ValidCredentials_ReturnsTrue()
        {
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct;
            AuthenticationFailureCode failureCode;
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct, out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.None, failureCode);
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct, out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.None, failureCode);
        }

        [TestMethod]
        public void AuthenticateWithUsernameOrEmail_SecuritySettingsEmailIsUsername_ReturnsCorrectValue()
        {
            configuration.EmailIsUsername = true;
            configuration.RequireAccountVerification = false;
            subject.CreateAccount("test", "pass", "test@test.com");
            UserAccount acct;
            AuthenticationFailureCode failureCode;
            Assert.IsFalse(subject.AuthenticateWithUsernameOrEmail("test", "pass", out acct, out failureCode));
            Assert.IsTrue(subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct, out failureCode));
            Assert.AreEqual(AuthenticationFailureCode.None, failureCode);
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
        public void AuthenticateWithUsernameOrEmail_EmailIsNotUnique_Throws()
        {
            configuration.EmailIsUnique = false;
            try
            {
                UserAccount acct;
                subject.AuthenticateWithUsernameOrEmail("test@test.com", "pass", out acct);
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
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

        [TestMethod]
        public void AuthenticationFailureCode_GetValidationMessage()
        {
            var failureCodes = GetEnumToSymbols<AuthenticationFailureCode>().Except(new[] { AuthenticationFailureCode.None });
            foreach (AuthenticationFailureCode code in failureCodes)
            {
                Assert.IsFalse(String.IsNullOrEmpty(subject.GetValidationMessage(code)));
            }
        }

        static IEnumerable<TEnum> GetEnumToSymbols<TEnum>() where TEnum : struct
        {
            return (from f in typeof(TEnum).GetFields() 
                    where !f.IsSpecialName 
                    select f.GetRawConstantValue()).Cast<TEnum>();
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
            subject.AddCertificate(acct.ID, cert);

            Assert.IsTrue(subject.AuthenticateWithCertificate(cert));
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndValidAccountID_ReturnsTrue()
        {
            var cert = GetTestCert();

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.AddCertificate(acct.ID, cert);

            Assert.IsTrue(subject.AuthenticateWithCertificate(acct.ID, cert));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_ValidCert_ReturnsCorrectAccount()
        {
            var cert = GetTestCert();

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.AddCertificate(acct.ID, cert);

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
            subject.AddCertificate(acct.ID, cert);

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
            subject.AddCertificate(acct.ID, cert);

            Assert.IsFalse(subject.AuthenticateWithCertificate(cert2));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_InvalidCert_ReturnsFalse()
        {
            var cert = GetTestCert();
            var cert2 = new X509Certificate2();

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.AddCertificate(acct.ID, cert);

            Assert.IsFalse(subject.AuthenticateWithCertificate(cert2));
        }

        [TestMethod]
        public void AuthenticateWithCertificate_WrongCertAndValidAccountId_ReturnsFalse()
        {
            var cert = GetTestCert();
            var cert2 = GetTestCert2();

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.AddCertificate(acct.ID, cert);

            Assert.IsFalse(subject.AuthenticateWithCertificate(acct.ID, cert2));
        }
        
        [TestMethod]
        public void AuthenticateWithCertificate_ValidCertAndInvalidAccountId_Throws()
        {
            var cert = GetTestCert();

            configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.AddCertificate(acct.ID, cert);

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

            var acct = repository.GetByID(id);
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
            subject.AddCertificate(acct.ID, cert);
            var id = acct.ID;

            subject.ConfigureTwoFactorAuthentication(id, TwoFactorAuthMode.Certificate);

            acct = repository.GetByID(id);
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
        public void SetPassword_AccountLocked_ResetsLockoutAndUserCanLogin()
        {
            this.configuration.RequireAccountVerification = false;
            this.configuration.AccountLockoutFailedLoginAttempts = 5;
            this.configuration.AccountLockoutDuration = TimeSpan.FromMinutes(1);

            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.IsTrue(subject.Authenticate("test", "pass"));
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            subject.Authenticate("test", "bad");
            Assert.IsFalse(subject.Authenticate("test", "pass"));
            subject.SetPassword(acct.ID, "newPass");
            Assert.IsTrue(subject.Authenticate("test", "newPass"));
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
                Assert.AreEqual(Resources.ValidationMessages.InvalidOldPassword, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.InvalidOldPassword, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.InvalidNewPassword, ex.Message);
            }
        }

        [TestMethod]
        public void ResetPassword_AllowsPasswordToBeReset()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

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
                Assert.AreEqual(Resources.ValidationMessages.InvalidEmail, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.InvalidEmail, ex.Message);
            }
        }

        [TestMethod]
        public void ResetPasswordFromEmail_EmailIsNotUnique_Fails()
        {
            configuration.EmailIsUnique = false;

            try
            {
                subject.ResetPassword("test2@test.com");
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
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

            try
            {
                subject.ChangePasswordFromResetKey("", "pass2");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidKey, ex.Message);
            }
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
        public void ChangePasswordFromResetKey_EmptyNewPass_Fails()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            configuration.RequireAccountVerification = false;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;

            subject.ResetPassword("test@test.com");
            var key = LastVerificationKey;

            try
            {
                subject.ChangePasswordFromResetKey(key, "");
                Assert.Fail();
            }
            catch(ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.PasswordRequired, ex.Message);
            }
        }

        [TestMethod]
        public void ChangePasswordFromResetKey_ResetsFailedLoginCount()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(this.LastVerificationKey, "pass");

            subject.Authenticate("test", "bad_pass");

            var account = subject.GetByID(id);
            Assert.AreEqual(1, account.FailedLoginCount);

            subject.ResetPassword("test@test.com");
            var key = LastVerificationKey;
            subject.ChangePasswordFromResetKey(key, "new_pass");

            account = subject.GetByID(id);
            Assert.AreEqual(0, account.FailedLoginCount);
        }

        [TestMethod]
        public void ResetFailedLoginCount_ResetsFailedLoginCount()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(this.LastVerificationKey, "pass");

            subject.Authenticate("test", "bad_pass");

            var account = subject.GetByID(id);
            Assert.AreEqual(1, account.FailedLoginCount);

            subject.ResetFailedLoginCount(id);
            
            account = subject.GetByID(id);
            Assert.AreEqual(0, account.FailedLoginCount);
        }

        [TestMethod]
        public void ChangeUsername_ChangesUsername()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.ChangeUsername(id, "test2");

            Assert.AreEqual("test2", repository.GetByID(id).Username);
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
                Assert.AreEqual(Resources.ValidationMessages.UsernameAlreadyInUse, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.InvalidUsername, ex.Message);
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
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
            var acct = repository.GetByID(id);
            Assert.AreEqual("test2@test.com", acct.Email);
        }
        
        [TestMethod]
        public void ChangeEmailRequest_NonuniqueEmail_FailsValidation()
        {
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass"); 
            var acct2 = subject.CreateAccount("test2", "pass", "test2@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            try
            {
                subject.ChangeEmailRequest(acct1.ID, "test2@test.com");
                Assert.Fail();
            }
            catch(ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void ChangeEmailRequest_EmailIsNotUnique_CanChangeEmailToSameEmailAsAnotherAccount()
        {
            configuration.EmailIsUnique = false;

            var id1 = subject.CreateAccount("test1", "pass", "test1@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
            var id2 = subject.CreateAccount("test2", "pass", "test2@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            subject.ChangeEmailRequest(id1, "test2@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
            var acct = repository.GetByID(id1);
            Assert.AreEqual("test2@test.com", acct.Email);
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
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            try
            {
                subject.ChangeEmailRequest(id, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailRequired, ex.Message);
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_Success_ReturnsTrue()
        {
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");
        }

        [TestMethod]
        public void ChangeEmailFromKey_SecuritySettingsEmailIsUsername_ChangesUsername()
        {
            configuration.EmailIsUsername = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            subject.ChangeEmailRequest(id, "test2@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            Assert.AreEqual("test2@test.com", repository.GetByID(id).Username);
        }

        [TestMethod]
        public void ChangeEmailFromKey_EmptyPassword_ValidationFails()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.VerifyEmailFromKey(LastVerificationKey, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidPassword, ex.Message);
            }
        }

        [TestMethod]
        public void ChangeEmailFromKey_InvalidPassword_ValidationFails()
        {
            configuration.AllowLoginAfterAccountCreation = true;
            var id = subject.CreateAccount("test", "pass", "test@test.com").ID;
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            subject.ChangeEmailRequest(id, "test2@test.com");
            try
            {
                subject.VerifyEmailFromKey(LastVerificationKey, "pass2");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidPassword, ex.Message);
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
                subject.VerifyEmailFromKey("", "pass");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.InvalidKey, ex.Message);
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

            Assert.IsNull(repository.GetByID(id).MobilePhoneNumber);
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

            Assert.AreEqual("123", repository.GetByID(id).MobilePhoneNumber);
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
                Assert.AreEqual(Resources.ValidationMessages.MobilePhoneAlreadyInUse, ex.Message);
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
                Assert.AreEqual(Resources.ValidationMessages.MobilePhoneAlreadyInUse, ex.Message);
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

            try
            {
                subject.ChangeMobilePhoneFromCode(id, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.CodeRequired, ex.Message);
            }
        }

        [TestMethod]
        public void RemoveLinkedAccount_NoPassword_CantRemoveLastLinkedAccount()
        {
            var acct = subject.CreateAccount("test", null, "test@test.com");
            subject.AddOrUpdateLinkedAccount(acct, "google", "123", null);

            try
            {
                subject.RemoveLinkedAccount(acct.ID, "google", "123");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.CantRemoveLastLinkedAccountIfNoPassword, ex.Message);
            }
        }
        [TestMethod]
        public void RemoveLinkedAccount_NoPassword_CanRemoveSecondToLastLinkedAccount()
        {
            var acct = subject.CreateAccount("test", null, "test@test.com");
            subject.AddOrUpdateLinkedAccount(acct, "google", "123", null);
            subject.AddOrUpdateLinkedAccount(acct, "facebook", "123", null);
            
            Assert.IsNotNull(subject.GetByLinkedAccount("google", "123"));

            subject.RemoveLinkedAccount(acct.ID, "google", "123");

            Assert.IsNull(subject.GetByLinkedAccount("google", "123"));
        }

        [TestMethod]
        public void AddOrUpdateLinkedAccount_ExternalProviderAlreadyInUseOnAnotherAccount_Fails()
        {
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            subject.AddOrUpdateLinkedAccount(acct1, "foo", "123");
            var acct2 = subject.CreateAccount("test2", "pass", "test2@test.com");
            try
            {
                subject.AddOrUpdateLinkedAccount(acct2, "foo", "123");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.LinkedAccountAlreadyInUse, ex.Message);
            }
        }
        [TestMethod]
        public void AddOrUpdateLinkedAccount_ExternalProviderAlreadyAssociatedWithAccount_Succeeds()
        {
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            subject.AddOrUpdateLinkedAccount(acct1, "foo", "123");
            subject.AddOrUpdateLinkedAccount(acct1, "foo", "123");
        }

        [TestMethod]
        public void RequestAccountVerification_InvalidID_Throws()
        {
            try
            {
                subject.RequestAccountVerification(Guid.Empty);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                Assert.AreSame("Invalid Account ID", ex.Message);
            }
        }

        [TestMethod]
        public void RequestAccountVerification_RaisesEmailChangeRequestedEvent()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.RequestAccountVerification(acct.ID);
            Assert.IsTrue(LastEvent is EmailChangeRequestedEvent<UserAccount>);
            var evt = LastEvent as EmailChangeRequestedEvent<UserAccount>;
            Assert.AreEqual(evt.Account.ID, acct.ID);
        }

        [TestMethod]
        public void RequestAccountVerification_EmptyEmail_FailsValidation()
        {
            subject.Configuration.RequireAccountVerification = false;
            var acct = subject.CreateAccount("test", "pass", null);

            try
            {
                subject.RequestAccountVerification(acct.ID);
                Assert.Fail();
            }
            catch(ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailRequired, ex.Message);
            }
        }

        [TestMethod]
        public void RequestAccountVerification_AccountVerified_Validation()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.VerifyEmailFromKey(LastVerificationKey, "pass");

            try
            {
                subject.RequestAccountVerification(acct.ID);
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.AccountAlreadyVerified, ex.Message);
            }
        }

        [TestMethod]
        public void CreateAccount_CustomPasswordValidation_Executes()
        {
            configuration.RegisterPasswordValidator((svc, acct, password) =>
            {
                return new ValidationResult("Bad Password");
            });
            try
            {
                subject.CreateAccount("test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual("Bad Password", ex.Message);
            }
        }
        [TestMethod]
        public void CreateAccount_CustomEmailValidation_Executes()
        {
            configuration.RegisterEmailValidator((svc, acct, password) =>
            {
                return new ValidationResult("Bad Email");
            });
            try
            {
                subject.CreateAccount("test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual("Bad Email", ex.Message);
            }
        }
        [TestMethod]
        public void CreateAccount_CustomUsernameValidation_Executes()
        {
            configuration.RegisterUsernameValidator((svc, acct, password) =>
            {
                return new ValidationResult("Bad Username");
            });
            try
            {
                subject.CreateAccount("test", "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual("Bad Username", ex.Message);
            }
        }

        [TestMethod]
        public void CustomValidationMessages_Used()
        {
            configuration.AddCommandHandler(delegate(GetValidationMessage cmd)
            {
                if (cmd.ID == MembershipRebootConstants.ValidationMessages.UsernameRequired)
                {
                    cmd.Message = "name required test";
                }
            });

            try
            {
                subject.CreateAccount(null, "pass", "test@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreSame("name required test", ex.Message);
            }

        }

        [TestMethod]
        public void SetConfirmedEmail_AccountIsVerified()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.SetConfirmedEmail(acct.ID, "test@test.com");
            Assert.IsTrue(subject.GetByID(acct.ID).IsAccountVerified);
        }

        [TestMethod]
        public void SetConfirmedEmail_EmailAlreadyInUse_Fails()
        {
            subject.CreateAccount("test1", "pass", "test1@test.com");
            var acct = subject.CreateAccount("test2", "pass", "test2@test.com");
            try
            {
                subject.SetConfirmedEmail(acct.ID, "test1@test.com");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailAlreadyInUse, ex.Message);
            }
        }
        
        [TestMethod]
        public void SetConfirmedEmail_EmailIsNotUnique_ForAccountWithExistingEmail_Succeeds()
        {
            configuration.EmailIsUnique = false;

            subject.CreateAccount("test1", "pass", "test1@test.com");
            var acct = subject.CreateAccount("test2", "pass", "test2@test.com");
            subject.SetConfirmedEmail(acct.ID, "test1@test.com");

            acct = subject.GetByID(acct.ID);
            Assert.IsTrue(acct.IsAccountVerified);
        }

        [TestMethod]
        public void SetConfirmedEmail_EmptyEmail_Fails()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            try
            {
                subject.SetConfirmedEmail(acct.ID, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.EmailRequired, ex.Message);
            }
        }

        [TestMethod]
        public void SetConfirmedEmail_ConfigurationEmailIsUsername_SetsUsername()
        {
            configuration.EmailIsUsername = true;
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.SetConfirmedEmail(acct.ID, "test2@test.com");
            Assert.AreEqual("test2@test.com", subject.GetByID(acct.ID).Username);
        }

        [TestMethod]
        public void SetConfirmedMobilePhone_SetsMobile()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.SetConfirmedMobilePhone(acct.ID, "123");
            Assert.AreEqual("123", subject.GetByID(acct.ID).MobilePhoneNumber);
        }

        [TestMethod]
        public void SetConfirmedMobilePhone_SamePhone_Fails()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.SetConfirmedMobilePhone(acct.ID, "123");
            try
            {
                subject.SetConfirmedMobilePhone(acct.ID, "123");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.MobilePhoneMustBeDifferent, ex.Message);
            }
        }

        [TestMethod]
        public void SetConfirmedMobilePhone_EmptyPhone_Fails()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            try
            {
                subject.SetConfirmedMobilePhone(acct.ID, "");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.MobilePhoneRequired, ex.Message);
            }
        }
        
        [TestMethod]
        public void SetConfirmedMobilePhone_PhoneAlreadyInUse_Fails()
        {
            var acct1 = subject.CreateAccount("test1", "pass", "test1@test.com");
            subject.SetConfirmedMobilePhone(acct1.ID, "123");
            var acct2 = subject.CreateAccount("test2", "pass", "test2@test.com");
            try
            {
                subject.SetConfirmedMobilePhone(acct2.ID, "123");
                Assert.Fail();
            }
            catch (ValidationException ex)
            {
                Assert.AreEqual(Resources.ValidationMessages.MobilePhoneAlreadyInUse, ex.Message);
            }
        }

        [TestMethod]
        public void AddClaim_AddsTheClaim()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            Assert.AreEqual(0, acct.Claims.Count());
            subject.AddClaim(acct.ID, "foo", "bar");
            acct = subject.GetByID(acct.ID);
            Assert.AreEqual(1, acct.Claims.Count());
            var claim = acct.Claims.First();
            Assert.AreEqual("foo", claim.Type);
            Assert.AreEqual("bar", claim.Value);
        }

        [TestMethod]
        public void RemoveClaim_RemovesTheClaim()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.AddClaim(acct.ID, "foo", "bar");
            acct = subject.GetByID(acct.ID);
            Assert.AreEqual(1, acct.Claims.Count());

            subject.RemoveClaim(acct.ID, "foo", "bar");
            acct = subject.GetByID(acct.ID);
            Assert.AreEqual(0, acct.Claims.Count());
        }

        [TestMethod]
        public void UpdateClaims_AddsAndRemovesClaims()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            subject.UpdateClaims(acct.ID,
                new UserClaimCollection() {
                    {"foo1", "bar1"},
                    {"foo2", "bar2"},
                    {"foo3", "bar3"},
                });
            Assert.AreEqual(3, subject.GetByID(acct.ID).Claims.Count());

            subject.UpdateClaims(acct.ID,
                new UserClaim[] {
                    new UserClaim("foo4", "bar4"),
                    new UserClaim("foo5", "bar5"),
                    new UserClaim("foo6", "bar6"),
                });
            Assert.AreEqual(6, subject.GetByID(acct.ID).Claims.Count());

            var claims = new Claim[]
            {
                new Claim("foo4", "bar4"),
                new Claim("foo5", "bar5"),
                new Claim("foo6", "bar6"),
            };

            subject.UpdateClaims(acct.ID, claims);
            Assert.AreEqual(6, subject.GetByID(acct.ID).Claims.Count());

            subject.UpdateClaims(acct.ID,
                new UserClaim[] {
                    new UserClaim("foo4", "bar99"),
                    new UserClaim("foo5", "bar99"),
                    new UserClaim("foo6", "bar99"),
                });
            Assert.AreEqual(9, subject.GetByID(acct.ID).Claims.Count());
            
            subject.UpdateClaims(acct.ID,
                null,
                new UserClaim[] {
                    new UserClaim("foo4", "bar99"),
                    new UserClaim("foo5", "bar99"),
                    new UserClaim("foo6", "bar99"),
                });
            Assert.AreEqual(6, subject.GetByID(acct.ID).Claims.Count());

            subject.UpdateClaims(acct.ID,
                null,
                new UserClaim[] {
                    new UserClaim("foo4", "bar99"),
                    new UserClaim("foo5", "bar99"),
                    new UserClaim("foo6", "bar99"),
                });
            Assert.AreEqual(6, subject.GetByID(acct.ID).Claims.Count());

            subject.UpdateClaims(acct.ID,
                null,
                new UserClaimCollection() {
                    {"foo1", "bar1"},
                    {"foo2", "bar2"},
                    {"foo3", "bar3"},
                });
            Assert.AreEqual(3, subject.GetByID(acct.ID).Claims.Count());

            subject.UpdateClaims(acct.ID, claims.Where(x=>false));
            Assert.AreEqual(3, subject.GetByID(acct.ID).Claims.Count());
        }

        [TestMethod]
        public void UpdateClaims_NoChanges_AccountNotUpdated()
        {
            var acct = subject.CreateAccount("test", "pass", "test@test.com");
            var lastUpdated = acct.LastUpdated;
            subject.UpdateClaims(acct.ID, null, Enumerable.Empty<UserClaim>().ToCollection());
            Assert.AreEqual(lastUpdated, subject.GetByID(acct.ID).LastUpdated);
        }

        [TestMethod]
        public void SendUsernameReminder_EmailIsNotUnique_Fails()
        {
            configuration.EmailIsUnique = false;
            try
            {
                subject.SendUsernameReminder("test@test.com");
                Assert.Fail();
            }
            catch (InvalidOperationException) { }
        }

        [TestMethod]
        public void Logging_in_before_account_is_verified_does_not_cause_account_to_have_last_login_set()
        {
            configuration.RequireAccountVerification = true;

            var acct = subject.CreateAccount("test", "pass", "test@test.com");

            var result = subject.Authenticate("test", "pass");
            Assert.IsFalse(result);

            acct = subject.GetByID(acct.ID);
            Assert.IsTrue(acct.IsNew());
        }




    }
}
