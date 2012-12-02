using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    class EFMembershipRebootDatabase : DbContext
    {
        static EFMembershipRebootDatabase()
        {
            Database.SetInitializer<EFMembershipRebootDatabase>(new EFMembershipRebootDatabaseInitializer());
        }

        public EFMembershipRebootDatabase()
        {
        }

        public EFMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);

        }
        
        public DbSet<UserAccount> Users { get; set; }
    }
}
