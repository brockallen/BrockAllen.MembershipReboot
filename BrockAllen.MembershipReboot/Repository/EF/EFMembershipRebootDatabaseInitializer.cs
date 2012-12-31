using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    class EFMembershipRebootDatabaseInitializer : CreateDatabaseIfNotExists<EFMembershipRebootDatabase>
    {
        public static void SeedContext(EFMembershipRebootDatabase context)
        {
#if DEBUG
            var adminAccount = UserAccount.Create("default", "admin", "admin123", "brockallen@gmail.com");
            adminAccount.VerifyAccount(adminAccount.VerificationKey);
            adminAccount.AddClaim(ClaimTypes.Role, "Administrator");
            context.Users.Add(adminAccount);
#endif 
        }

        protected override void Seed(EFMembershipRebootDatabase context)
        {
            SeedContext(context);
            base.Seed(context);
        }
    }
}
