
namespace BrockAllen.MembershipReboot
{
    public class EFUserAccountRepository : DbContextRepository<UserAccount, int, EFMembershipRebootDatabase>, IUserAccountRepository
    {

    }

    public class EFUserAccountRepository<TKey>
        : DbContextRepository<UserAccount<TKey>, TKey, EFMembershipRebootDatabase>, IUserAccountRepository<TKey>
    {
        public EFUserAccountRepository()
        {
        }

        public EFUserAccountRepository(string name)
            : base(new EFMembershipRebootDatabase(name))
        {
        }
    }
}
