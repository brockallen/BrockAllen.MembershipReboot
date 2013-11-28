/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository<TAccount> : IRepository<TAccount>
        where TAccount : UserAccount
    {
    }
    
    public interface IUserAccountRepository : IUserAccountRepository<UserAccount> { }
}
