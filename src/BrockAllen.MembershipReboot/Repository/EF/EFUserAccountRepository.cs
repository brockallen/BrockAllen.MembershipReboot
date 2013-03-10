
namespace BrockAllen.MembershipReboot
{
    public class EFUserAccountRepository 
        : DbContextRepository<UserAccount, int, EFMembershipRebootDatabase>, IUserAccountRepository
    {
        public EFUserAccountRepository()
        {
        }

        public EFUserAccountRepository(string name) : base(new EFMembershipRebootDatabase(name))
        {
        }
    }
}
