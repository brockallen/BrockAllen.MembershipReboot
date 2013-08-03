
namespace BrockAllen.MembershipReboot
{
    public class EventBusUserAccountRepository :
       EventBusRepository<UserAccount>, IUserAccountRepository
    {
        public EventBusUserAccountRepository(IUserAccountRepository inner, IEventBus eventBus)
            : base(inner, eventBus)
        {
        }
    }
}
