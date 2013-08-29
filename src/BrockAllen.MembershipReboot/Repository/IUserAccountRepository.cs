/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository : IRepository<UserAccount>
    {
        UserAccount FindByLinkedAccount(string tenant, string provider, string id);
    }
}
