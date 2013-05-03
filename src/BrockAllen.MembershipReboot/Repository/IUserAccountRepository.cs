namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository : IUserAccountRepository<UserAccount, int>
    {
    }

    public interface IUserAccountRepository<T, TKey> : IRepository<T, TKey>
        where T : UserAccount<TKey>
    {
    }
}
