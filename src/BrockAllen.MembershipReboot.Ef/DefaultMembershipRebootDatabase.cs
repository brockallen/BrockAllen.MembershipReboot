/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Relational;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultMembershipRebootDatabase : DbContext
    {
        public DefaultMembershipRebootDatabase()
            : this("MembershipReboot", null)
        {
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString)
            : this(nameOrConnectionString, null)
        {
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString, string schemaName)
            : base(nameOrConnectionString)
        {
            this.SchemaName = schemaName;
            this.RegisterUserAccountChildTablesForDelete<RelationalUserAccount>();
            this.RegisterGroupChildTablesForDelete<RelationalGroup>();
        }

        public string SchemaName { get; private set; }

        public DbSet<RelationalUserAccount> Users { get; set; }
        public DbSet<RelationalGroup> Groups { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureMembershipRebootUserAccounts<RelationalUserAccount>(this.SchemaName);
            modelBuilder.ConfigureMembershipRebootGroups<RelationalGroup>(this.SchemaName);
        }
    }
}
