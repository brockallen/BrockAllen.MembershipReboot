using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class SmtpMessageDelivery : IMessageDelivery
    {
        public void Send(Message msg)
        {
            if (String.IsNullOrWhiteSpace(msg.From))
            {
                SmtpSection smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                msg.From = smtp.From;
            }

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Send(msg.From, msg.To, msg.Subject, msg.Body);
            }
        }
    }
}
