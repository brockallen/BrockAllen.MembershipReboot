/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;

namespace BrockAllen.MembershipReboot
{
    public class SmtpDeliveryConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string FromEmailAddress { get; set; }
    }
    
    public class SmtpMessageDelivery : IMessageDelivery
    {
        public bool SendAsHtml { get; set; }
        public int SmtpTimeout { get; set; }
        public SmtpDeliveryConfig Config { get; set; }

        public SmtpMessageDelivery(SmtpDeliveryConfig config, bool sendAsHtml = false, int smtpTimeout = 5000)
        {
            if(config == null) throw new ArgumentNullException("config");
            this.Config = config;
            this.SendAsHtml = sendAsHtml;
            this.SmtpTimeout = smtpTimeout;
        }
        
        public void Send(Message msg)
        {
            Tracing.Information("[SmtpMessageDelivery.Send] sending mail to " + msg.To);

            if (String.IsNullOrWhiteSpace(msg.From))
            {
                msg.From = Config.FromEmailAddress;
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
                    smtp.UseDefaultCredentials = false;
                    smtp.Host = Config.Host;
                    smtp.Credentials = new NetworkCredential(Config.UserName, Config.Password);
                    smtp.Port = Config.Port;
                    smtp.EnableSsl = Config.EnableSsl;

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
