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
            : base("name=MembershipReboot")
        {
            this.RegisterUserAccountChildTablesForDelete<RelationalUserAccount>();
            this.RegisterGroupChildTablesForDelete<RelationalGroup>();
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            this.RegisterUserAccountChildTablesForDelete<RelationalUserAccount>();
            this.RegisterGroupChildTablesForDelete<RelationalGroup>();
        }

        public DbSet<RelationalUserAccount> Users { get; set; }
        public DbSet<RelationalGroup> Groups { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureMembershipRebootUserAccounts<RelationalUserAccount>();
            modelBuilder.ConfigureMembershipRebootGroups<RelationalGroup>();
        }
    }
}
