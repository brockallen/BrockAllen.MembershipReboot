
namespace BrockAllen.MembershipReboot
{
    public class EFLinkedAccountRepository 
        : DbContextRepository<LinkedAccount, EFMembershipRebootDatabase>, ILinkedAccountRepository
    {
        public EFLinkedAccountRepository()
        {
        }

        public EFLinkedAccountRepository(string name)
            : base(new EFMembershipRebootDatabase(name))
        {
        }
    }
}
