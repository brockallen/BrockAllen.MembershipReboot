namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository : IUserAccountRepositoryBase<UserAccount, int>
    {
    }

    public interface IUserAccountRepository<TKey>
        : IUserAccountRepositoryBase<UserAccount<TKey>, TKey>
    {
    }

    public interface IUserAccountRepositoryBase<T, TKey> : IRepository<T, TKey>
        where T : class
    {
    }
}
