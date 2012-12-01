using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class NopMessageDelivery : IMessageDelivery
    {
        public void Send(Message msg)
        {
        }
    }
}
