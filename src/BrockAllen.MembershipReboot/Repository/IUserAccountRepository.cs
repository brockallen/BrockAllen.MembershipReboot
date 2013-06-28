/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository 
        : IRepository<UserAccount>
    {
    }

    public class UserAccountRepository :
        EventBusRepository<UserAccount>, IUserAccountRepository
    {
        public UserAccountRepository(IUserAccountRepository inner, IEventBus eventBus)
            : base(inner, eventBus)
        {
        }
    }
}
