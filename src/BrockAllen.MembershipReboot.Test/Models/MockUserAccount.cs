/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Moq;
using System;

namespace BrockAllen.MembershipReboot.Test.Models
{
    public class MockUserAccount : Mock<UserAccount>
    {
        public MockUserAccount(string tenant, string username, string password, string email, DateTime now)
        {
            this.Setup(x => x.UtcNow).Returns(now);
            this.CallBase = true;
            this.Object.Init(tenant, username, password, email);
        }
        
        public MockUserAccount(string tenant, string username, string password, string email)
        {
            this.CallBase = true;
            this.Object.Init(tenant, username, password, email);
        }

        public MockUserAccount()
        {
            this.CallBase = true;
        }
    }
}
