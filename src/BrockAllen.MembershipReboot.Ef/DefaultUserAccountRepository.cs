/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultUserAccountRepository
           : DbContextUserAccountRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultUserAccountRepository()
        {
        }

        public DefaultUserAccountRepository(string name)
            : this(new DefaultMembershipRebootDatabase(name))
        {
        }

        public DefaultUserAccountRepository(DefaultMembershipRebootDatabase ctx)
            : base(ctx)
        {
        }
    }
}
