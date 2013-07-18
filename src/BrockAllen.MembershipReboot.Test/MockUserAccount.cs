/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Moq;
using System;

namespace BrockAllen.MembershipReboot.Test
{
    public class MockUserAccount : Mock<UserAccount>
    {
        string password;

        public MockUserAccount(string tenant, string username, string password, string email, DateTime? now = null)
        {
            this.password = password;
            if (now != null ) this.Setup(x => x.UtcNow).Returns(now.Value);
            this.CallBase = true;
            this.Object.Init(tenant, username, password, email);
        }

        public MockUserAccount()
        {
            this.CallBase = true;
        }

        public void VerifyAccount()
        {
            this.Object.IsAccountVerified = true;
            this.Object.ClearVerificationKey();
        }
    }
}
