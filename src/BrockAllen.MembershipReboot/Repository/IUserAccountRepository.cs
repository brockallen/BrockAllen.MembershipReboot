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
        IQueryable<IUserAccount> GetAll();
        IUserAccount Get(Guid id);
        IUserAccount Create();
        void Add(IUserAccount item);
        void Remove(IUserAccount item);
        void Update(IUserAccount item);
    }
}
