using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EFMembershipRebootDatabase : DbContext
    {
        public EFMembershipRebootDatabase()
            : base("name=MembershipReboot")
        {
        }

        public EFMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<UserAccount> Users { get; set; }
    }
}
