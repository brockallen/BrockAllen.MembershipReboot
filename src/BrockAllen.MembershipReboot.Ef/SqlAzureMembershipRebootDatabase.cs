using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace BrockAllen.MembershipReboot.Ef
{
    public class SqlAzureMembershipRebootDatabase : DefaultMembershipRebootDatabase
    {
        public SqlAzureMembershipRebootDatabase()
            : base("name=MembershipReboot")
        {
        }

        public SqlAzureMembershipRebootDatabase(string nameOrConnectionString)
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
