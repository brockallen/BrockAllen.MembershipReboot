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
        private readonly bool sendAsHtml;

        public SmtpMessageDelivery(bool sendAsHtml = false)
        {
            this.sendAsHtml = sendAsHtml;
        }

        public void Send(Message msg)
        {
            if (String.IsNullOrWhiteSpace(msg.From))
            {
                SmtpSection smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                msg.From = smtp.From;
            }

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Timeout = 5000;
                try
                {
                    MailMessage mailMessage = new MailMessage(msg.From, msg.To, msg.Subject, msg.Body)
                                                        {
                                                            IsBodyHtml = sendAsHtml
                                                        };
                    smtp.Send(mailMessage);
                }
                catch (SmtpException e)
                {
                    Tracing.Error("[SmtpMessageDelivery.Send] SmtpException: " + e.Message);
                }
                catch (Exception e)
                {
                    Tracing.Error("[SmtpMessageDelivery.Send] Exception: " + e.Message);
                }
            }
        }
    }
}
