using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.EF
{
    class EFMembershipRebootDatabaseInitializer : CreateDatabaseIfNotExists<EFMembershipRebootDatabase>
    {
        protected override void Seed(EFMembershipRebootDatabase context)
        {
            UserAccount admin =
                UserAccount.Create("admin", "admin123", "admin@admin.com");
            admin.AddClaim(ClaimTypes.Role, "Administrator");
            context.Users.Add(admin);

            base.Seed(context);
        }
    }
}
