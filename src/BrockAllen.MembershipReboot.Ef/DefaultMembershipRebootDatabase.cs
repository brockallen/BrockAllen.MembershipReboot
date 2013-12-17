/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultMembershipRebootDatabase : DbContext
    {
        public DefaultMembershipRebootDatabase()
            : base("name=MembershipReboot")
        {
        }

        public DefaultMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<UserAccount> Users { get; set; }
        public DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureMembershipRebootUserAccounts<UserAccount>();
            modelBuilder.ConfigureMembershipRebootGroups<Group>();
        }
    }
}
