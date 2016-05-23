/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Relational;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class MembershipRebootDbContext<TUserAccount, TGroup> : DbContext
        where TUserAccount : RelationalUserAccount
        where TGroup : RelationalGroup
    {
        public MembershipRebootDbContext()
            : this("MembershipReboot", null)
        {
        }

        public MembershipRebootDbContext(string nameOrConnectionString)
            : this(nameOrConnectionString, null)
        {
        }

        public MembershipRebootDbContext(string nameOrConnectionString, string schemaName)
            : base(nameOrConnectionString)
        {
            this.SchemaName = schemaName;
            this.RegisterUserAccountChildTablesForDelete<TUserAccount>();
            this.RegisterGroupChildTablesForDelete<TGroup>();
        }

        public string SchemaName { get; private set; }

        public DbSet<TUserAccount> Users { get; set; }
        public DbSet<TGroup> Groups { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureMembershipRebootUserAccounts<TUserAccount>(this.SchemaName);
            modelBuilder.ConfigureMembershipRebootGroups<TGroup>(this.SchemaName);
        }
    }

    public class MembershipRebootDbContext<TUserAccount> : MembershipRebootDbContext<TUserAccount, RelationalGroup>
        where TUserAccount : RelationalUserAccount
    {
        public MembershipRebootDbContext()
            : base()
        {
        }

        public MembershipRebootDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString, null)
        {
        }

        public MembershipRebootDbContext(string nameOrConnectionString, string schemaName)
            : base(nameOrConnectionString, schemaName)
        {
        }
    }
}
