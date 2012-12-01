using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class SmtpMessageDelivery : IMessageDelivery
    {
        public void Send(Message msg)
        {
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Send(null, msg.To, msg.Subject, msg.Body);
            }
        }
    }
}
