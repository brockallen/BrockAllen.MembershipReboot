using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrockAllen.MembershipReboot.Test.Services.Accounts
{
    [TestClass]
    public class UserAccountServiceTests
    {
        public class MockUserAccountService
        {
            public Mock<IUserAccountRepository> UserAccountRepository { get; set; }
            public Mock<INotificationService> NotificationService { get; set; }
            public Mock<IPasswordPolicy> PasswordPolicy { get; set; }

            public MockUserAccountService()
            {
                this.UserAccountRepository = new Mock<IUserAccountRepository>();
            }

            Mock<UserAccountService> svc;
            public UserAccountService Object
            {
                get
                {
                    return Mock.Object;
                }
            }
            public Mock<UserAccountService> Mock
            {
                get
                {
                    if (svc == null)
                    {
                        svc = new Mock<UserAccountService>(UserAccountRepository.Object, NotificationService != null ? NotificationService.Object : null, PasswordPolicy != null ? PasswordPolicy.Object : null);
                        svc.CallBase = true;
                    }
                    return svc;
                }
            }

            internal void MockUserAccounts(params UserAccount[] accounts)
            {
                this.UserAccountRepository.Setup(x => x.GetAll()).Returns(accounts.AsQueryable());
            }
        }

        [TestClass]
        public class Ctor
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullUserAccountRepo_Throws()
            {
                var sub = new UserAccountService(null, null, null);
            }
        }

        [TestClass]
        public class Dispose
        {
            [TestMethod]
            public void CallsDisposeOnUserRepo()
            {
                var userAccountRepo = new Mock<IUserAccountRepository>();
                var sub = new UserAccountService(userAccountRepo.Object, null, null);
                sub.Dispose();
                userAccountRepo.Verify(x => x.Dispose());
            }
        }

        [TestClass]
        public class SaveChanges
        {
            [TestMethod]
            public void CallsSaveChangesOnRepository()
            {
                var repo = new Mock<IUserAccountRepository>();
                var sub = new UserAccountService(repo.Object, null, null);
                sub.SaveChanges();
                repo.Verify(x => x.SaveChanges());
            }
        }

        [TestClass]
        public class GetAll
        {
            [TestInitialize]
            public void Init()
            {
                SecuritySettings.Instance = new SecuritySettings();
            }

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
                SecuritySettings.Instance.MultiTenant = true;
                var sub = new MockUserAccountService();
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
                SecuritySettings.Instance.MultiTenant = false;
                SecuritySettings.Instance.DefaultTenant = "a";
                var sub = new MockUserAccountService();
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
                SecuritySettings.Instance.MultiTenant = false;
                SecuritySettings.Instance.DefaultTenant = "a";
                var sub = new MockUserAccountService();
                sub.MockUserAccounts(
                    new UserAccount { Tenant = "a" },
                    new UserAccount { Tenant = "a" },
                    new UserAccount { Tenant = "b" });
                var result = sub.Object.GetAll(null);
                Assert.AreEqual(2, result.Count());
            }
        }
    }
}
