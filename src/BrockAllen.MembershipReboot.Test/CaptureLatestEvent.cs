namespace BrockAllen.MembershipReboot.Test
{
    public class CaptureLatestEvent<TEvent, TAccount> : IEventHandler<TEvent> where TEvent: UserAccountEvent<TAccount>
    {
        public TEvent Latest { get; set; }
        public void Handle(TEvent evt)
        {
            Latest = evt;
        }
    }

    public static class CaptureLatestEvent
    {
        public static CaptureLatestEvent<TEvent, UserAccount> For<TEvent>() where TEvent : UserAccountEvent<UserAccount>
        {
            return new CaptureLatestEvent<TEvent, UserAccount>();
        }
    }
}