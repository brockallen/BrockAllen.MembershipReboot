/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountQuery
    {
        IEnumerable<string> GetAllTenants();
        IEnumerable<UserAccountQueryResult> Query(string filter);
        IEnumerable<UserAccountQueryResult> Query(string tenant, string filter);
        IEnumerable<UserAccountQueryResult> Query(string filter, int skip, int count, out int totalCount);
        IEnumerable<UserAccountQueryResult> Query(string tenant, string filter, int skip, int count, out int totalCount);
    }
}
