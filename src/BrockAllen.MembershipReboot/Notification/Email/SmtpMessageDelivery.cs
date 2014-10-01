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
        public bool SendAsHtml { get; set; }
        public int SmtpTimeout { get; set; }

        public SmtpMessageDelivery(bool sendAsHtml = false, int smtpTimeout = 5000)
        {
            this.SendAsHtml = sendAsHtml;
            this.SmtpTimeout = smtpTimeout;
        }

        public void Send(Message msg)
        {
            Tracing.Information("[SmtpMessageDelivery.Send] sending mail to " + msg.To);

            if (String.IsNullOrWhiteSpace(msg.From))
            {
                SmtpSection smtp = ConfigurationManager.GetSection("system.net/mailSettings/smtp") as SmtpSection;
                msg.From = smtp.From;
            }

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Timeout = SmtpTimeout;
                try
                {
                    MailMessage mailMessage = new MailMessage(msg.From, msg.To, msg.Subject, msg.Body)
                    {
                        IsBodyHtml = SendAsHtml
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
