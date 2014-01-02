/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Collections.Generic;
namespace BrockAllen.MembershipReboot
{
    public interface IGroupQuery
    {
        IEnumerable<string> GetAllTenants();
        IEnumerable<string> GetRoleNames(string tenant);
        IEnumerable<GroupQueryResult> Query(string filter);
        IEnumerable<GroupQueryResult> Query(string tenant, string filter);
        IEnumerable<GroupQueryResult> Query(string filter, int skip, int count, out int totalCount);
        IEnumerable<GroupQueryResult> Query(string tenant, string filter, int skip, int count, out int totalCount);
    }
}
