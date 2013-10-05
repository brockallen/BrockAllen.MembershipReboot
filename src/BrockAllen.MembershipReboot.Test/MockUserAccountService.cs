/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Linq;
using Moq;

namespace BrockAllen.MembershipReboot.Test
{
    public class MockUserAccountService
    {
        public SecuritySettings SecuritySettings { get; set; }
        public MembershipRebootConfiguration Configuration { get; set; }
        public Mock<IUserAccountRepository> UserAccountRepository { get; set; }

        public MockUserAccountService()
        {
            this.UserAccountRepository = new Mock<IUserAccountRepository>();
            this.UserAccountRepository.Setup(x => x.Create()).Returns(new UserAccount());

            this.SecuritySettings = new SecuritySettings();
            //Configuration = new MembershipRebootConfiguration(this.SecuritySettings, this.UserAccountRepository.Object);
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
                    svc = new Mock<UserAccountService>(this.Configuration);
                    svc.CallBase = true;
                }
                return svc;
            }
        }

        internal void MockUserAccounts(params UserAccount[] accounts)
        {
            this.UserAccountRepository.Setup(x => x.GetAll()).Returns(accounts.AsQueryable());
        }
        internal void MockUserAccounts(params MockUserAccount[] mocks)
        {
            MockUserAccounts(mocks.Select(x => x.Object).ToArray());
        }
    }
}
