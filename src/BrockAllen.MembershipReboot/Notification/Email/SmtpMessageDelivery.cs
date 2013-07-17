/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;

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
