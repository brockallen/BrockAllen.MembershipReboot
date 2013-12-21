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
<<<<<<< HEAD
                smtp.Timeout = 10000;
                try
                {
                    
=======
                smtp.Timeout = 5000;
                try
                {
>>>>>>> upstream/master
                    smtp.Send(msg.From, msg.To, msg.Subject, msg.Body);
                }
                catch (SmtpException e)
                {
<<<<<<< HEAD
                    Tracing.Verbose("Error in Send Mail: " + e.Message);
                }
                catch (Exception e) {
                    Tracing.Verbose("Error in Send Mail: " + e.Message);
=======
                    Tracing.Error("[SmtpMessageDelivery.Send] SmtpException: " + e.Message);
                }
                catch (Exception e)
                {
                    Tracing.Error("[SmtpMessageDelivery.Send] Exception: " + e.Message);
>>>>>>> upstream/master
                }
            }
        }
    }
}
