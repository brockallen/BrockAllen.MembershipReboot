using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.EF
{
    class EFMembershipRebootDatabase : DbContext
    {
        public DbSet<UserAccount> UserAcounts { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
    }
}
