
namespace BrockAllen.MembershipReboot
{
    public class EventBusUserAccountRepository :
       EventBusRepository<UserAccount>, IUserAccountRepository
    {
        public EventBusUserAccountRepository(IUserAccountRepository inner, IEventBus validationBus, IEventBus eventBus)
            : base(inner, validationBus, eventBus)
        {
        }
    }
}
