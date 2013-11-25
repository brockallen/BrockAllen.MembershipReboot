/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository : IDisposable
    {
        IQueryable<UserAccount> GetAll();
        UserAccount Get(Guid id);
        UserAccount Create();
        void Add(UserAccount item);
        void Remove(UserAccount item);
        void Update(UserAccount item);
    }
}
