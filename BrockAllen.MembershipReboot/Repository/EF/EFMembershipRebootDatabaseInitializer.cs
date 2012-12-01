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
        protected override void Seed(EFMembershipRebootDatabase context)
        {
            UserAccount admin =
                UserAccount.Create("admin", "admin123", "brockallen@gmail.com");
            admin.VerifyAccount(admin.VerificationKey);
            admin.AddClaim(ClaimTypes.Role, "Administrator");
            context.Users.Add(admin);

            base.Seed(context);
        }
    }
}
