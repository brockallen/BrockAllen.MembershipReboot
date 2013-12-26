namespace BrockAllen.MembershipReboot.Nh
{
    using BrockAllen.MembershipReboot;

    public class NhUserAccountService<TAccount> : UserAccountService<TAccount> where TAccount : NhUserAccount
    {
        public NhUserAccountService(IUserAccountRepository<TAccount> userRepository)
            : base(userRepository)
        {
        }

        public NhUserAccountService(MembershipRebootConfiguration<TAccount> configuration, IUserAccountRepository<TAccount> userRepository)
            : base(configuration, userRepository)
        {
        }
    }

    public class NhUserAccountService : NhUserAccountService<NhUserAccount>
    {
        public NhUserAccountService(IUserAccountRepository<NhUserAccount> userRepository)
            : this(new MembershipRebootConfiguration<NhUserAccount>(), userRepository)
        {
        }

        public NhUserAccountService(MembershipRebootConfiguration<NhUserAccount> configuration, IUserAccountRepository<NhUserAccount> userRepository)
            : base(configuration, userRepository)
        {
        }
    }
}