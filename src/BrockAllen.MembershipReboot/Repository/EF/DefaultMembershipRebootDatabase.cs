/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;

namespace BrockAllen.MembershipReboot
{
    public class DefaultMembershipRebootDatabase : DbContext
    {
        public DefaultMembershipRebootDatabase()
            : this("name=" + SecuritySettings.Instance.ConnectionStringName)
        {
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<UserAccount> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
    }

    public class DefaultUserAccountRepository
        : DbContextUserAccountRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultUserAccountRepository()
            : this(SecuritySettings.Instance.ConnectionStringName)
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

    public class DefaultRoleRepository
        : DbContextRoleRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultRoleRepository()
            : this(SecuritySettings.Instance.ConnectionStringName)
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
