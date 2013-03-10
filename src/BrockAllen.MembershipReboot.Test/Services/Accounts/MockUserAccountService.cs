using System.Linq;
using Moq;

namespace BrockAllen.MembershipReboot.Test.Services.Accounts
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
}
