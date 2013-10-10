using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.Accounts
{
    [TestClass]
    public class UserAccountServiceTests
    {
        static Guid a = Guid.NewGuid();
        static Guid b = Guid.NewGuid();
        static Guid c = Guid.NewGuid();
        static Guid d = Guid.NewGuid();

        [TestClass]
        public class Ctor
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullConfig_Throws()
            {
                //var sub = new UserAccountService((MembershipRebootConfiguration)null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullUserAccountRepo_Throws()
            {
                //var sub = new UserAccountService(null, null, null);
            }
        }

        [TestClass]
        public class Dispose
        {
            //[TestMethod]
            //public void CallsDisposeOnUserRepo()
            //{
            //    var userAccountRepo = new Mock<IUserAccountRepository>();
            //    //var sub = new UserAccountService(userAccountRepo.Object, null, null);
            //    //sub.Dispose();
            //    userAccountRepo.Verify(x => x.Dispose());
            //}
        }

        [TestClass]
        public class Update
        {
            //[TestMethod]
            //public void CallsUpdateOnRepository()
            //{
            //    var repo = new Mock<IUserAccountRepository>();
            //    var sub = new UserAccountService(repo.Object, null, null);
            //    var ua = new UserAccount();
            //    sub.Update(ua);
            //    repo.Verify(x => x.Update(ua));
            //}
        }

        [TestClass]
        public class ValidateUsername
        {
            [TestMethod]
            public void CallsAllValidators()
            {
                bool wasCalled = false;
                var sub = new MockUserAccountService();
                sub.Configuration.RegisterUsernameValidator(
                    new DelegateValidator((svc, ua, val) =>
                    {
                        wasCalled = true;
                        return null;
                    }));
                sub.Object.ValidateUsername(new MockUserAccount().Object, "foo");
                Assert.IsTrue(wasCalled);
            }
            
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void ValidatorFailure_Throws()
            {
                var mockVal = new Mock<IValidator>();
                mockVal.Setup(x => x.Validate(It.IsAny<UserAccountService>(), It.IsAny<UserAccount>(), It.IsAny<String>())).Returns(new ValidationResult("error"));
                var sub = new MockUserAccountService();
                sub.Configuration.RegisterUsernameValidator(mockVal.Object);

                sub.Object.ValidateUsername(new MockUserAccount().Object, "foo");
            }
        }
        [TestClass]
        public class ValidateEmail
        {
            [TestMethod]
            public void CallsAllValidators()
            {
                bool wasCalled = false;
                var sub = new MockUserAccountService();
                sub.Configuration.RegisterEmailValidator(
                    new DelegateValidator((svc, ua, val) =>
                    {
                        wasCalled = true;
                        return null;
                    }));
                sub.Object.ValidateEmail(new MockUserAccount().Object, "foo@foo.com");
                Assert.IsTrue(wasCalled);
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void ValidatorFailure_Throws()
            {
                var mockVal = new Mock<IValidator>();
                mockVal.Setup(x => x.Validate(It.IsAny<UserAccountService>(), It.IsAny<UserAccount>(), It.IsAny<String>())).Returns(new ValidationResult("error"));
                var sub = new MockUserAccountService();
                sub.Configuration.RegisterEmailValidator(mockVal.Object);
                
                sub.Object.ValidateEmail(new MockUserAccount().Object, "foo@foo.com");
            }
        }
        [TestClass]
        public class ValidatePassword
        {
            [TestMethod]
            public void CallsAllValidators()
            {
                bool wasCalled = false;
                var sub = new MockUserAccountService();
                sub.Configuration.RegisterPasswordValidator(
                    new DelegateValidator((svc, ua, val) =>
                    {
                        wasCalled = true;
                        return null;
                    }));
                sub.Object.ValidatePassword(new MockUserAccount().Object, "foo");
                Assert.IsTrue(wasCalled);
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void ValidatorFailure_Throws()
            {
                var mockVal = new Mock<IValidator>();
                mockVal.Setup(x => x.Validate(It.IsAny<UserAccountService>(), It.IsAny<UserAccount>(), It.IsAny<String>())).Returns(new ValidationResult("error"));
                var sub = new MockUserAccountService();
                sub.Configuration.RegisterPasswordValidator(mockVal.Object);

                sub.Object.ValidatePassword(new MockUserAccount().Object, "foo");
            }
        }

        [TestClass]
        public class GetAll
        {
            [TestMethod]
            public void NoParams_CallsGetAllWithNullTenant()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetAll();
                sub.Mock.Verify(x => x.GetAll(null));
            }

            [TestMethod]
            public void MultiTenantEnabled_NullTenant_ReturnsEmptyResults()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                sub.MockUserAccounts(
                    new UserAccount { Tenant = "a" },
                    new UserAccount { Tenant = "a" },
                    new UserAccount { Tenant = "b" });
                var result = sub.Object.GetAll(null);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public void MultiTenantNotEnabled_NullTenant_ReturnsResultsForDefaultTenant()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = false;
                sub.SecuritySettings.DefaultTenant = "a";
                sub.MockUserAccounts(
                    new UserAccount { Tenant = "a" },
                    new UserAccount { Tenant = "a" },
                    new UserAccount { Tenant = "b" });
                
                var result = sub.Object.GetAll(null);
                
                Assert.AreEqual(2, result.Count());
            }

            [TestMethod]
            public void SomeAccountsClosed_ReturnsOnlyAccountsNotClosed()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, IsAccountClosed = true },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, IsAccountClosed = true },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, },
                    new UserAccount { ID = d, Tenant = sub.SecuritySettings.DefaultTenant, });
                var result = sub.Object.GetAll(null);
                Assert.AreEqual(2, result.Count());
                CollectionAssert.AreEquivalent(new Guid[] { c, d }, result.Select(x => x.ID).ToArray());
            }
        }

        [TestClass]
        public class GetByUsername
        {
            [TestMethod]
            public void OnlyPassUsername_PassesNullTenant()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetByUsername("test");
                sub.Mock.Verify(x => x.GetByUsername(null, "test"));
            }
            [TestMethod]
            public void MultiTenantEnabled_PassNullTenant_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                var result = sub.Object.GetByUsername(null, "test");
                Assert.IsNull(result);
            }
            [TestMethod]
            public void PassNullUsername_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetByUsername(null);
                Assert.IsNull(result);
            }
            [TestMethod]
            public void PassValidUsername_ReturnsCorrectResult()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Username = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Username = "b" },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, Username = "c" });
                var result = sub.Object.GetByUsername("b");
                Assert.AreEqual(b, result.ID);
            }
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PassValidUsername_MultipleMatches_Throws()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Username = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Username = "a" },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, Username = "c" });
                sub.Object.GetByUsername("a");
            }
            [TestMethod]
            public void PassInvalidUsername_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Username = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Username = "b" },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, Username = "c" });
                var result = sub.Object.GetByUsername("d");
                Assert.IsNull(result);
            }
        }

        [TestClass]
        public class GetByEmail
        {
            [TestMethod]
            public void OnlyPassEmail_PassesNullTenant()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetByEmail("test@test.com");
                sub.Mock.Verify(x => x.GetByEmail(null, "test@test.com"));
            }
            [TestMethod]
            public void MultiTenantEnabled_PassNullTenant_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                var result = sub.Object.GetByEmail(null, "test@test.com");
                Assert.IsNull(result);
            }
            [TestMethod]
            public void PassNullEmail_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetByEmail(null);
                Assert.IsNull(result);
            }
            [TestMethod]
            public void PassValidEmail_ReturnsCorrectResult()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Email = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Email = "b" },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, Email = "c" });
                var result = sub.Object.GetByEmail("b");
                Assert.AreEqual(b, result.ID);
            }
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PassValidEmail_MultipleMatches_Throws()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Email = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Email = "a" },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, Email = "c" });
                sub.Object.GetByEmail("a");
            }
            [TestMethod]
            public void PassInvalidEmail_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Email = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Email = "b" },
                    new UserAccount { ID = c, Tenant = sub.SecuritySettings.DefaultTenant, Email = "c" });
                var result = sub.Object.GetByEmail("d");
                Assert.IsNull(result);
            }
        }

        [TestClass]
        public class GetByID
        {
            [TestMethod]
            public void GuidParam_CallsRepositoryGetAndReturnsAccount()
            {
                var sub = new MockUserAccountService();
                var ua = new UserAccount();
                sub.UserAccountRepository.Setup(x => x.Get(a)).Returns(ua);
                var result = sub.Object.GetByID(a);
                sub.UserAccountRepository.Verify(x => x.Get(a));
                Assert.AreSame(ua, result);
            }

            //[TestMethod]
            //public void StringParam_CallsRepositoryGetAndReturnsAccount()
            //{
            //    var sub = new MockUserAccountService();
            //    var ua = new UserAccount();
            //    sub.UserAccountRepository.Setup(x => x.Get(a)).Returns(ua);
            //    var result = sub.Object.GetByID(a.ToString());
            //    sub.UserAccountRepository.Verify(x => x.Get(a));
            //    Assert.AreSame(ua, result);
            //}
            //[TestMethod]
            //public void BadStringParam_ReturnsNull()
            //{
            //    var sub = new MockUserAccountService();
            //    var ua = new UserAccount();
            //    sub.UserAccountRepository.Setup(x => x.Get(a)).Returns(ua);
            //    var result = sub.Object.GetByID("not a guid");
            //    Assert.IsNull(result);
            //}
        }

        [TestClass]
        public class GetByVerificationKey
        {
            [TestMethod]
            public void NullKey_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetByVerificationKey(null);
                Assert.IsNull(result);
            }
            [TestMethod]
            public void EmptyKey_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.GetByVerificationKey("");
                Assert.IsNull(result);
            }

            [TestMethod]
            public void ValidKey_ReturnsCorrectResult()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, VerificationKey = "a" },
                    new UserAccount { ID = b, VerificationKey = "b" },
                    new UserAccount { ID = c, VerificationKey = "c" });
                var result = sub.Object.GetByVerificationKey("b");
                Assert.AreEqual(b, result.ID);
            }
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void ValidKey_MultipleMatches_Throws()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, VerificationKey = "a" },
                    new UserAccount { ID = b, VerificationKey = "b" },
                    new UserAccount { ID = c, VerificationKey = "b" });
                var result = sub.Object.GetByVerificationKey("b");
            }
            [TestMethod]
            public void InvalidKey_ReturnsNull()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, VerificationKey = "a" },
                    new UserAccount { ID = b, VerificationKey = "b" },
                    new UserAccount { ID = c, VerificationKey = "c" });
                var result = sub.Object.GetByVerificationKey("d");
                Assert.IsNull(result);
            }
        }

        [TestClass]
        public class UsernameExists
        {
            [TestMethod]
            public void PassJustUsername_PassesNullForTenant()
            {
                var sub = new MockUserAccountService();
                sub.Object.UsernameExists("name");
                sub.Mock.Verify(x => x.UsernameExists(null, "name"));
            }
            [TestMethod]
            public void MultiTenantEnabled_NullTenantPassed_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                var result = sub.Object.UsernameExists(null, "name");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void NullUsernamePassed_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.UsernameExists(null);
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void UsernamesUniqueAcrossTenants_CorrectResultsReturned()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                sub.SecuritySettings.UsernamesUniqueAcrossTenants = true;
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = "t1", Username = "a" },
                    new UserAccount { ID = b, Tenant = "t1", Username = "b" },
                    new UserAccount { ID = c, Tenant = "t2", Username = "c" });
                Assert.IsTrue(sub.Object.UsernameExists("a"));
                Assert.IsTrue(sub.Object.UsernameExists("t1", "a"));
                Assert.IsTrue(sub.Object.UsernameExists("t2", "a"));
                Assert.IsTrue(sub.Object.UsernameExists("t3", "a"));

                Assert.IsFalse(sub.Object.UsernameExists("d"));
                Assert.IsFalse(sub.Object.UsernameExists("t1", "d"));
                Assert.IsFalse(sub.Object.UsernameExists("t2", "d"));
                Assert.IsFalse(sub.Object.UsernameExists("t3", "d"));
            }
            [TestMethod]
            public void UsernamesNotUniqueAcrossTenants_CorrectResultsReturned()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = "t1", Username = "a" },
                    new UserAccount { ID = b, Tenant = "t1", Username = "b" },
                    new UserAccount { ID = c, Tenant = "t2", Username = "a" },
                    new UserAccount { ID = d, Tenant = sub.SecuritySettings.DefaultTenant, Username = "d" });
                Assert.IsTrue(sub.Object.UsernameExists("t1", "a"));
                Assert.IsTrue(sub.Object.UsernameExists("t1", "b"));
                Assert.IsTrue(sub.Object.UsernameExists("t2", "a"));
                Assert.IsTrue(sub.Object.UsernameExists(sub.SecuritySettings.DefaultTenant, "d"));

                Assert.IsFalse(sub.Object.UsernameExists("t1", "c"));
                Assert.IsFalse(sub.Object.UsernameExists("t2", "b"));
                Assert.IsFalse(sub.Object.UsernameExists("t2", "c"));
            }
        }

        [TestClass]
        public class EmailExists
        {
            [TestMethod]
            public void NoTenant_PassesNullTenant()
            {
                var sub = new MockUserAccountService();
                sub.Object.EmailExists("email");
                sub.Mock.Verify(x => x.EmailExists(null, "email"));
            }
            [TestMethod]
            public void MultiTenantEnabled_NullTenantParam_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                var result = sub.Object.EmailExists(null, "email");
                Assert.IsFalse(result);
            }
            [TestMethod]
            public void NullEmailParam_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.EmailExists(null);
                Assert.IsFalse(result);
            }

            [TestMethod]
            public void MultiTenantEnabled_ReturnsCorrectValues()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = "t1", Email = "a" },
                    new UserAccount { ID = b, Tenant = "t1", Email = "b" },
                    new UserAccount { ID = c, Tenant = "t2", Email = "a" });
                Assert.IsTrue(sub.Object.EmailExists("t1", "a"));
                Assert.IsTrue(sub.Object.EmailExists("t1", "b"));
                Assert.IsTrue(sub.Object.EmailExists("t2", "a"));

                Assert.IsFalse(sub.Object.EmailExists("t2", "b"));
                Assert.IsFalse(sub.Object.EmailExists("t2", "c"));
                Assert.IsFalse(sub.Object.EmailExists("t3", "a"));
                Assert.IsFalse(sub.Object.EmailExists("a"));
            }

            [TestMethod]
            public void MultiTenantNotEnabled_ReturnsCorrectValues()
            {
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { ID = a, Tenant = sub.SecuritySettings.DefaultTenant, Email = "a" },
                    new UserAccount { ID = b, Tenant = sub.SecuritySettings.DefaultTenant, Email = "b" },
                    new UserAccount { ID = c, Tenant = "t2", Email = "a" });

                Assert.IsTrue(sub.Object.EmailExists("a"));
                Assert.IsTrue(sub.Object.EmailExists("b"));
                Assert.IsTrue(sub.Object.EmailExists(sub.SecuritySettings.DefaultTenant, "a"));
                Assert.IsTrue(sub.Object.EmailExists("t1", "b"));
                Assert.IsTrue(sub.Object.EmailExists("t2", "a"));

                Assert.IsFalse(sub.Object.EmailExists("c"));
                Assert.IsFalse(sub.Object.EmailExists("t2", "c"));
            }
        }

        [TestClass]
        public class CreateAccount
        {
            [TestMethod]
            public void NoTenantPassed_PassesNullTenant()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("user", "pass", "email@test.com");
                sub.Mock.Verify(x => x.CreateAccount(null, "user", "pass", "email@test.com"));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MultiTenantEnabled_NullTenant_Throws()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                sub.Object.CreateAccount(null, "user", "pass", "email@test.com");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullUsername_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("tenant", null, "pass", "email@test.com");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullPassword_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("tenant", "user", null, "email@test.com");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullEmail_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("tenant", "user", "pass", null);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MultiTenant_EmptyTenant_Throws()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;
                sub.Object.CreateAccount("", "user", "pass", "email@test.com");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyUsername_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("tenant", "", "pass", "email@test.com");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyPassword_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("tenant", "user", "", "email@test.com");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmptyEmail_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("tenant", "user", "pass", "");
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void InvalidEmail_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.CreateAccount("user", "pass", "invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void UsernameAlreadyExists_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.UsernameExists(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
                sub.Object.CreateAccount("user", "pass", "email@test.com");
            }

            [TestMethod]
            public void UsernameAlreadyExists_EmailIsUsername_HasEmailInTheMessage()
            {
                try
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.EmailIsUsername = true;
                    
                    sub.Mock.Setup(x => x.EmailExists(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
                    sub.Object.CreateAccount("user", "pass", "email@test.com");
                }
                catch (ValidationException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("Email"));
                }
                catch
                {
                    throw;
                }
            }
            [TestMethod]
            public void UsernameAlreadyExists_EmailIsNotUsername_HasUsernameInTheMessage()
            {
                try
                {
                    var sub = new MockUserAccountService();
                    sub.Mock.Setup(x => x.UsernameExists(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
                    sub.Object.CreateAccount("user", "pass", "email@test.com");
                }
                catch (ValidationException ex)
                {
                    Assert.IsTrue(ex.Message.StartsWith("Username"));
                }
                catch
                {
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void EmailAlreadyExists_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.EmailExists(It.IsAny<string>(), It.Is<string>(e => e.CompareTo("email@test.com") == 0))).Returns(true);
                sub.Object.CreateAccount("user", "pass", "email@test.com");
            }

            [TestMethod]
            public void ValidAccount_AddedToRepository()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                sub.UserAccountRepository.Verify(x => x.Add(result));
            }

            [TestMethod]
            public void ValidAccount_ReturnsAccount()
            {
                var sub = new MockUserAccountService();
                var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                Assert.IsNotNull(result);
                Assert.AreEqual("user", result.Username);
                Assert.AreEqual("email@test.com", result.Email);
            }

            [TestMethod]
            public void IsAccountVerified_SetProperly()
            {
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.RequireAccountVerification = true;

                    var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                    Assert.IsFalse(result.IsAccountVerified);
                }
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.RequireAccountVerification = false;

                    var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                    Assert.IsTrue(result.IsAccountVerified);
                }
            }

            [TestMethod]
            public void IsLoginAllowed_SetProperly()
            {
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.AllowLoginAfterAccountCreation = true;
                    
                    var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                    Assert.IsTrue(result.IsLoginAllowed);
                }
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.AllowLoginAfterAccountCreation = false;

                    var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                    Assert.IsFalse(result.IsLoginAllowed);
                }
            }

            [TestMethod]
            public void Verification_SetProperly()
            {
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.RequireAccountVerification = true;
                    var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                    Assert.IsNotNull(result.VerificationKey);
                    Assert.IsNotNull(result.VerificationKeySent);
                }
                {
                    var sub = new MockUserAccountService();
                    sub.SecuritySettings.RequireAccountVerification = false;
                    var result = sub.Object.CreateAccount("user", "pass", "email@test.com");
                    Assert.IsNull(result.VerificationKey);
                    Assert.IsNull(result.VerificationKeySent);
                }
            }
        }

        [TestClass]
        public class VerifyAccount
        {
            [TestMethod]
            public void InvalidKey_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns((UserAccount)null);
                Assert.IsFalse(sub.Object.VerifyAccount("key"));
            }
            [TestMethod]
            public void Success_CallsVerifyAccountOnUserAccountAndSaveOnRepository()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);

                sub.Object.VerifyAccount("key");

                account.Verify(x => x.VerifyAccount("key"));
                sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            }

            [TestMethod]
            public void VerifyAccountReturnsTrue_ReturnsTrue()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Setup(x => x.VerifyAccount(It.IsAny<string>())).Returns(true);
                Assert.IsTrue(sub.Object.VerifyAccount("key"));
            }

            [TestMethod]
            public void VerifyAccountReturnsFalse_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Setup(x => x.VerifyAccount(It.IsAny<string>())).Returns(false);
                Assert.IsFalse(sub.Object.VerifyAccount("key"));
            }
        }

        [TestClass]
        public class CancelNewAccount
        {
            [TestMethod]
            public void InvalidKey_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns((UserAccount)null);
                Assert.IsFalse(sub.Object.CancelNewAccount("key"));
            }
            [TestMethod]
            public void AccountVerified_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Object.IsAccountVerified = true;

                Assert.IsFalse(sub.Object.CancelNewAccount("key"));
            }
            [TestMethod]
            public void PurposeDoesntMatch_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Object.VerificationKey = "key1";

                Assert.IsFalse(sub.Object.CancelNewAccount("key1"));
            }
            [TestMethod]
            public void KeysDontMatch_ReturnsFalse()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                account.Object.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Object.VerificationKey = "key1";

                Assert.IsFalse(sub.Object.CancelNewAccount("key2"));
            }
            [TestMethod]
            public void KeysMatch_ReturnsTrue()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                account.Object.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Object.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                account.Object.VerificationKey = "key";

                Assert.IsTrue(sub.Object.CancelNewAccount("key"));
            }
            [TestMethod]
            public void KeysMatch_DeleteAccountCalled()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                account.Object.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                account.Object.VerificationPurpose = VerificationKeyPurpose.VerifyAccount;
                account.Object.VerificationKey = "key";

                sub.Object.CancelNewAccount("key");

                sub.Mock.Verify(x => x.DeleteAccount(account.Object));
            }
        }

        [TestClass]
        public class DeleteAccount
        {
            //[TestMethod]
            //public void NoTenantParam_PassesNullTenant()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.DeleteAccount("user");
            //    sub.Mock.Verify(x => x.DeleteAccount(null, "user"));
            //}
            //[TestMethod]
            //public void MultiTenantEnabled_NullTenantParam_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.MultiTenant = true;

            //    var result = sub.Object.DeleteAccount(null, "user");
            //    Assert.IsFalse(result);
            //}
            //[TestMethod]
            //public void NullUsernameParam_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    var result = sub.Object.DeleteAccount((string)null);
            //    Assert.IsFalse(result);
            //}

            //[TestMethod]
            //public void NoAccountFound_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    var result = sub.Object.DeleteAccount("user");
            //    Assert.IsFalse(result);
            //}

            //[TestMethod]
            //public void AccountFound_ReturnsSuccess()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    var result = sub.Object.DeleteAccount("user");
            //    Assert.IsTrue(result);
            //}

            //[TestMethod]
            //public void AccountFound_DeleteAccountCalled()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    var result = sub.Object.DeleteAccount("user");
            //    sub.Mock.Verify(x => x.DeleteAccount(account.Object));
            //}

            [TestMethod]
            public void AllowAccountDeletion_CallsRemoveOnRepo()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.AllowAccountDeletion = true;

                var account = new MockUserAccount();
                sub.Object.DeleteAccount(account.Object);

                sub.UserAccountRepository.Verify(x => x.Remove(account.Object));
            }
            [TestMethod]
            public void AllowAccountDeletionDisabled_AccountIsNotVerified_CallsRemoveOnRepo()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.AllowAccountDeletion = false;

                var account = new MockUserAccount();
                account.Object.IsAccountVerified = false;
                sub.Object.DeleteAccount(account.Object);

                sub.UserAccountRepository.Verify(x => x.Remove(account.Object));
            }
            [TestMethod]
            public void AllowAccountDeletionDisabled_AccountIsVerified_CallsCloseOnAccount()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.AllowAccountDeletion = false;

                var account = new MockUserAccount();
                account.Object.IsLoginAllowed = true;
                account.Object.IsAccountVerified = true;
                sub.Object.DeleteAccount(account.Object);

                sub.UserAccountRepository.Verify(x => x.Remove(It.IsAny<UserAccount>()), Times.Never());
                account.Verify(x => x.CloseAccount());
                sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            }
        }

        [TestClass]
        public class Authenticate
        {
            [TestMethod]
            public void NoTenantParam_PassesNullForTenant()
            {
                var sub = new MockUserAccountService();
                sub.Object.Authenticate("user", "pass");
                sub.Mock.Verify(x => x.Authenticate(null, "user", "pass"));
            }

            [TestMethod]
            public void MultiTenantEnabled_NullTenantParam_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;

                Assert.IsFalse(sub.Object.Authenticate(null, "user", "pass"));
            }

            [TestMethod]
            public void NullUsername_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                Assert.IsFalse(sub.Object.Authenticate((string)null, "pass"));
            }
            [TestMethod]
            public void NullPassword_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                Assert.IsFalse(sub.Object.Authenticate("user", null));
            }

            [TestMethod]
            public void NoAccountFound_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns((UserAccount)null);
                Assert.IsFalse(sub.Object.Authenticate("user", "pass"));
            }

            //[TestMethod]
            //public void AccountFound_CallsAuthenticate()
            //{
            //    var sub = new MockUserAccountService();
            //    UserAccount account = new UserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account);
            //    sub.Object.Authenticate("user", "pass");
            //    sub.Mock.Verify(x => x.Authenticate(account, "pass"));
            //}

            //[TestMethod]
            //public void CallsAuthenticateOnUserAccount()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Object.Authenticate(account.Object, "pass");
            //    account.Verify(x => x.Authenticate("pass", It.IsAny<int>(), It.IsAny<TimeSpan>()));
            //}
            //[TestMethod]
            //public void CallsSaveChangesOnRepo()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Object.Authenticate(account.Object, "pass");
            //    sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            //}
            //[TestMethod]
            //public void userAccountReturnsTrue_ReturnsTrue()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    account.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>())).Returns(true);
            //    Assert.IsTrue(sub.Object.Authenticate(account.Object, "pass"));
            //}
            //[TestMethod]
            //public void userAccountReturnsFalse_ReturnsFalse()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    account.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<TimeSpan>())).Returns(false);
            //    Assert.IsFalse(sub.Object.Authenticate(account.Object, "pass"));
            //}
        }

        [TestClass]
        public class SetPassword
        {
            //[TestMethod]
            //public void NoTenantParam_PassesNullTenant()
            //{
            //    var sub = new MockUserAccountService();
            //    try
            //    {
            //        sub.Object.SetPassword("user", "pass");
            //    }
            //    catch { }
            //    sub.Mock.Verify(x => x.SetPassword(null, "user", "pass"));
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void MultiTenantEnabled_NullTenant_Throws()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.MultiTenant = true;

            //    sub.Object.SetPassword(null, "user", "pass");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void NullUsername_Throws()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.SetPassword(null, "pass");
            //}
            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void NullPassword_Throws()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.SetPassword("user", null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void NoUser_Throws()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.MockUserAccounts(new UserAccount { Tenant = "tenant", Username = "user" });
            //    sub.Object.SetPassword("tenant", "user2", "pass");
            //}
            //[TestMethod]
            //public void ValidUser_SetPasswordCalled()
            //{
            //    var sub = new MockUserAccountService();
            //    var user = new MockUserAccount(sub.SecuritySettings.DefaultTenant, "user", "foo", "user@foo.com");
            //    sub.MockUserAccounts(user.Object);
            //    sub.Object.SetPassword("user", "pass");
            //    user.Verify(x => x.SetPassword("pass"));
            //}
            //[TestMethod]
            //public void ValidUser_RepositorySaved()
            //{
            //    var sub = new MockUserAccountService();
            //    var user = new MockUserAccount(sub.SecuritySettings.DefaultTenant, "user", "foo", "user@foo.com");
            //    sub.MockUserAccounts(user.Object);
            //    sub.Object.SetPassword("user", "pass");
            //    sub.UserAccountRepository.Verify(x => x.Update(user.Object));
            //}
        }

        [TestClass]
        public class ChangePassword
        {
            //[TestMethod]
            //public void NoTenantParam_PassesNullTenant()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.ChangePassword("user", "old", "new");
            //    sub.Mock.Verify(x => x.ChangePassword(null, "user", "old", "new"));
            //}
            //[TestMethod]
            //public void MultiTenantEnabled_NullTenant_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.MultiTenant = true;

            //    Assert.IsFalse(sub.Object.ChangePassword(null, "user", "old", "new"));
            //}
            //[TestMethod]
            //public void NullUsername_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangePassword(null, "old", "new"));
            //}
            //[TestMethod]
            //public void NullOldPass_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangePassword("user", null, "new"));
            //}
            //[TestMethod]
            //public void NullNewPass_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangePassword("user", "old", null));
            //}
            //[TestMethod]
            //public void NoAccountFound_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns((UserAccount)null);
            //    Assert.IsFalse(sub.Object.ChangePassword("user", "old", "new"));
            //}
            //[TestMethod]
            //public void AccountFound_CallsAccountSetPassword()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    sub.Object.ChangePassword("user", "old", "new");
            //    account.Verify(x => x.SetPassword("new"));
            //}
            //[TestMethod]
            //public void RepositorySaveChangesCalled()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    sub.Object.ChangePassword("user", "old", "new");
            //    sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            //}
            //[TestMethod]
            //public void AuthenticateReturnsTrue_ReturnsTrue()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    Assert.IsTrue(sub.Object.ChangePassword("user", "old", "new"));
            //}
            //[TestMethod]
            //public void AuthenticateReturnsFalse_ReturnsFalse()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(false);
            //    Assert.IsFalse(sub.Object.ChangePassword("user", "old", "new"));
            //}
        }

        [TestClass]
        public class ResetPassword
        {
            [TestMethod]
            public void NoTenantParam_PassessNullForTenant()
            {
                var sub = new MockUserAccountService();
                try
                {
                    sub.Object.ResetPassword("email");
                }
                catch { }
                sub.Mock.Verify(x => x.ResetPassword(null, "email"));
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void MultiTenantEnabled_NullTenantParam_Throws()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;

                sub.Object.ResetPassword(null, "email");
            }
            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullEmail_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Object.ResetPassword(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NoAccountFound_Throws()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<string>())).Returns((UserAccount)null);
                sub.Object.ResetPassword("email");
            }

            [TestMethod]
            public void AccountVerified_UserAccountResetPasswordCalled()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
                account.Object.IsAccountVerified = true;
                sub.Object.ResetPassword("email");
                account.Verify(x => x.ResetPassword());
            }
            [TestMethod]
            public void UserAccountRepoSaveChangesCalled()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
                account.Object.IsAccountVerified = true;
                sub.Object.ResetPassword("email");
                sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            }
        }

        [TestClass]
        public class ChangePasswordFromResetKey
        {
            [TestMethod]
            public void NullKey_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                Assert.IsFalse(sub.Object.ChangePasswordFromResetKey(null, "new"));
            }
            [TestMethod]
            public void EmptyKey_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                Assert.IsFalse(sub.Object.ChangePasswordFromResetKey("", "new"));
            }
            [TestMethod]
            public void AccountNotFound_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns((UserAccount)null);
                Assert.IsFalse(sub.Object.ChangePasswordFromResetKey("key", "new"));
            }
            [TestMethod]
            public void ChangePasswordFromResetKeyCalledOnUserAccount()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                sub.Object.ChangePasswordFromResetKey("key", "new");
                account.Verify(x => x.ChangePasswordFromResetKey("key", "new"));
            }
            [TestMethod]
            public void UserAccountSuccess_ReturnsSuccess()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                sub.Object.ChangePasswordFromResetKey("key", "new");
                account.Setup(x => x.ChangePasswordFromResetKey(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                Assert.IsTrue(sub.Object.ChangePasswordFromResetKey("key", "new"));
            }
            [TestMethod]
            public void UserAccountFail_ReturnsFail()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                sub.Object.ChangePasswordFromResetKey("key", "new");
                account.Setup(x => x.ChangePasswordFromResetKey(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

                Assert.IsFalse(sub.Object.ChangePasswordFromResetKey("key", "new"));
            }
            [TestMethod]
            public void UserAccountSuccess_CallsSaveChangesOnUserAccountRepo()
            {
                var sub = new MockUserAccountService();
                var account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
                sub.Object.ChangePasswordFromResetKey("key", "new");
                account.Setup(x => x.ChangePasswordFromResetKey(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

                sub.Object.ChangePasswordFromResetKey("key", "new");

                sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            }
        }

        [TestClass]
        public class SendUsernameReminder
        {
            [TestMethod]
            public void NoTenantParam_PassesNullTenant()
            {
                var sub = new MockUserAccountService();
                try
                {
                    sub.Object.SendUsernameReminder("email");
                }
                catch { }
                sub.Mock.Verify(x => x.SendUsernameReminder(null, "email"));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MultiTenantEnabled_NullTenantParam_DoesNotSendAccountNameReminder()
            {
                var sub = new MockUserAccountService();
                sub.SecuritySettings.MultiTenant = true;

                sub.Object.SendUsernameReminder(null, "email");
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NullEmailParam_DoesNotSendAccountNameReminder()
            {
                var sub = new MockUserAccountService();
                sub.Object.SendUsernameReminder(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ValidationException))]
            public void NoAccountFound_DoesNotSendAccountNameReminder()
            {
                var sub = new MockUserAccountService();
                sub.Mock.Setup(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<string>())).Returns((UserAccount)null);
                sub.Object.SendUsernameReminder("email");
            }

            [TestMethod]
            public void AccountFound_DoesNotSendAccountNameReminder()
            {
                var sub = new MockUserAccountService();
                MockUserAccount account = new MockUserAccount();
                sub.Mock.Setup(x => x.GetByEmail(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
                sub.Object.SendUsernameReminder("email");
                account.Verify(x => x.SendAccountNameReminder());
            }
        }

        [TestClass]
        public class ChangeEmailRequest
        {
            //[TestMethod]
            //public void NoTenantParam_PassesNullTenant()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.ChangeEmailRequest("user", "email@test.com");
            //    sub.Mock.Verify(x => x.ChangeEmailRequest(null, "user", "email@test.com"));
            //}

            //[TestMethod]
            //public void MultiTenantEnabled_NullTenant_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.MultiTenant = true;

            //    Assert.IsFalse(sub.Object.ChangeEmailRequest(null, "user", "email@test.com"));
            //}
            //[TestMethod]
            //public void NullUsername_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangeEmailRequest(null, "email@test.com"));
            //}
            //[TestMethod]
            //public void NullEmail_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangeEmailRequest("user", null));
            //}
            //[TestMethod]
            //[ExpectedException(typeof(ValidationException))]
            //public void EmailNotValidFormat_Throws()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.ChangeEmailRequest("user", "email");
            //}
            //[TestMethod]
            //public void AccountNotFound_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns((UserAccount)null);
            //    Assert.IsFalse(sub.Object.ChangeEmailRequest("user", "email@test.com"));
            //}
            //[TestMethod]
            //public void AccountFound_CallsUserAccountChangeEmailRequest()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    sub.Object.ChangeEmailRequest("user", "email@test.com");
            //    account.Verify(x => x.ChangeEmailRequest("email@test.com"));
            //}
            //[TestMethod]
            //public void UserAccountChangeEmailRequestSuccess_ReturnsSuccess()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    account.Setup(x => x.ChangeEmailRequest(It.IsAny<string>())).Returns(true);
            //    Assert.IsTrue(sub.Object.ChangeEmailRequest("user", "email@test.com"));
            //}
            //[TestMethod]
            //public void UserAccountChangeEmailRequestFail_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    account.Setup(x => x.ChangeEmailRequest(It.IsAny<string>())).Returns(false);
            //    Assert.IsFalse(sub.Object.ChangeEmailRequest("user", "email@test.com"));
            //}
            //[TestMethod]
            //public void UserAccountChangeEmailRequestSuccess_CallsSaveChangesOnRepo()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    account.Setup(x => x.ChangeEmailRequest(It.IsAny<string>())).Returns(true);
            //    sub.Object.ChangeEmailRequest("user", "email@test.com");
            //    sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            //}

            //[TestMethod]
            //public void EmailIsUsername_AllowEmailChangeWhenEmailIsUsername_ReturnsSuccess()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.EmailIsUsername = true;

            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByUsername(It.IsAny<string>(), It.IsAny<string>())).Returns(account.Object);
            //    account.Setup(x => x.ChangeEmailRequest(It.IsAny<string>())).Returns(true);

            //    Assert.IsTrue(sub.Object.ChangeEmailRequest("user", "email@test.com"));
            //}
        }

        [TestClass]
        public class ChangeEmailFromKey
        {
            //[TestMethod]
            //public void NoLockoutParams_PassDefaultLockoutParams()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com");
            //    sub.Mock.Verify(x => x.ChangeEmailFromKey("pass", "key", "email@test.com"));
            //}


            //[TestMethod]
            //public void NullPass_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangeEmailFromKey(null, "key", "email@test.com"));
            //}
            //[TestMethod]
            //public void NullKey_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangeEmailFromKey("pass", null, "email@test.com"));
            //}
            //[TestMethod]
            //public void NullEmail_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.ChangeEmailFromKey("pass", "key", null));
            //}
            //[TestMethod]
            //public void AccountNotFound_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns((UserAccount)null);
            //    Assert.IsFalse(sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com"));
            //}
            //[TestMethod]
            //public void AuthenticateFails_ReturnsFail()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            //    Assert.IsFalse(sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com"));
            //}
            //[TestMethod]
            //public void UserAccountChangeEmailFromKeyCalled()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com");
            //    account.Verify(x => x.ChangeEmailFromKey("key", "email@test.com"));
            //}
            //[TestMethod]
            //public void UserAccountChangeEmailFromKeySuccess_ReturnSuccess()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    account.Setup(x => x.ChangeEmailFromKey(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            //    Assert.IsTrue(sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com"));
            //}
            //[TestMethod]
            //public void UserAccountChangeEmailFromKeyFail_ReturnFail()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    account.Setup(x => x.ChangeEmailFromKey(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            //    Assert.IsFalse(sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com"));
            //}

            //[TestMethod]
            //public void UserAccountChangeEmailFromKeySuccess_SaveChangesCalled()
            //{
            //    var sub = new MockUserAccountService();
            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    account.Setup(x => x.ChangeEmailFromKey(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            //    sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com");
            //    sub.UserAccountRepository.Verify(x => x.Update(account.Object));
            //}

            //[TestMethod]
            //public void EmailIsUsername_AllowEmailChangeWhenEmailIsUsername_WhenSuccess_UpdatesUsername()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.EmailIsUsername = true;

            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    account.Setup(x => x.ChangeEmailFromKey(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            //    sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com");

            //    Assert.AreEqual("email@test.com", account.Object.Username);
            //}

            //[TestMethod]
            //public void EmailIsUsername_AllowEmailChangeWhenEmailIsUsername_WhenFail_DoesNotUpdatesUsername()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.EmailIsUsername = true;

            //    var account = new MockUserAccount();
            //    sub.Mock.Setup(x => x.GetByVerificationKey(It.IsAny<string>())).Returns(account.Object);
            //    sub.Mock.Setup(x => x.Authenticate(It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(true);
            //    account.Setup(x => x.ChangeEmailFromKey(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            //    sub.Object.ChangeEmailFromKey("pass", "key", "email@test.com");

            //    Assert.AreNotEqual("email@test.com", account.Object.Username);
            //}
        }

        [TestClass]
        public class IsPasswordExpired
        {
            //[TestMethod]
            //public void NoTenantParam_PassesNullTenant()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.Object.IsPasswordExpired("user");
            //    sub.Mock.Verify(x => x.IsPasswordExpired(null, "user"));
            //}

            //[TestMethod]
            //public void MultiTenantEnabled_NullTenant_ReturnsFalse()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.MultiTenant = true;

            //    Assert.IsFalse(sub.Object.IsPasswordExpired(null, "user"));
            //}

            //[TestMethod]
            //public void NullUsername_ReturnsFalse()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.IsPasswordExpired("tenant", null));
            //}

            //[TestMethod]
            //public void NoAccount_ReturnsFalse()
            //{
            //    var sub = new MockUserAccountService();
            //    Assert.IsFalse(sub.Object.IsPasswordExpired("user"));
            //}

            //[TestMethod]
            //public void ValidAccount_CallsAccount()
            //{
            //    var sub = new MockUserAccountService();
            //    sub.SecuritySettings.DefaultTenant = "tenant";
            //    var account = new MockUserAccount("tenant", "user", "pass", "email@foo.com");
            //    sub.MockUserAccounts(account.Object);
            //    var result = sub.Object.IsPasswordExpired("tenant", "user");
            //    account.Verify(x => x.GetIsPasswordExpired(It.IsAny<int>()));
            //}

        }
    }
}
