/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextUserAccountRepository<Ctx>
           : DbContextRepository<UserAccount>, IUserAccountRepository
        where Ctx : DbContext, new()
    {
        public DbContextUserAccountRepository()
            : this(new Ctx())
        {
        }
        public DbContextUserAccountRepository(Ctx ctx)
            : base(ctx)
        {
        }

        public UserAccount FindByLinkedAccount(string tenant, string provider, string id) 
        {
            if (String.IsNullOrWhiteSpace(tenant)) return null;
            if (String.IsNullOrWhiteSpace(provider)) return null;
            if (String.IsNullOrWhiteSpace(id)) return null;

            IUserAccountRepository that = this;

            var query =
                from u in that.GetAll()
                where u.Tenant == tenant
                from l in u.LinkedAccounts
                where l.ProviderName == provider && l.ProviderAccountID == id
                select u;

            return query.SingleOrDefault();
        }
    }
}
