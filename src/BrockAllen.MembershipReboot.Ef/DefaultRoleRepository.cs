/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultRoleRepository
        : DbContextRoleRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultRoleRepository()
        {
        }

        public DefaultRoleRepository(string name)
            : this(new DefaultMembershipRebootDatabase(name))
        {
        }

        public DefaultRoleRepository(DefaultMembershipRebootDatabase ctx)
            : base(ctx)
        {
        }
    }
}
