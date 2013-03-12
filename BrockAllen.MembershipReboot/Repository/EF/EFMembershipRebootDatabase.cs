using System.Data.Entity;

namespace BrockAllen.MembershipReboot
{
    public class EFMembershipRebootDatabase : DbContext
    {
        public EFMembershipRebootDatabase()
            : base("name=" + SecuritySettings.Instance.ConnectionString)
        {
        }

        public EFMembershipRebootDatabase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<UserAccount> Users { get; set; }
    }
}
