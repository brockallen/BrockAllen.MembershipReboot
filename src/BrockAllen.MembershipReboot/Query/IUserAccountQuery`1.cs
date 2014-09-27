/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountQuery<TAccount>
        where TAccount : UserAccount
    {
        IEnumerable<UserAccountQueryResult> Query(Func<IQueryable<TAccount>, IQueryable<TAccount>> filter);
        IEnumerable<UserAccountQueryResult> Query(Func<IQueryable<TAccount>, IQueryable<TAccount>> filter, Func<IQueryable<TAccount>, IOrderedQueryable<TAccount>> sort, int skip, int count, out int totalCount);
    }
}
