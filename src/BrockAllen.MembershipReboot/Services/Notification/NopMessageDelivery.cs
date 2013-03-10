
namespace BrockAllen.MembershipReboot
{
    public class NopMessageDelivery : IMessageDelivery
    {
        public void Send(Message msg)
        {
        }
    }
}
