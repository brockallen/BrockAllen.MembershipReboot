/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.IO;

namespace BrockAllen.MembershipReboot
{
    public class SmsMessageFormatter : IMessageFormatter
    {
        Lazy<ApplicationInformation> appInfo;
        public SmsMessageFormatter(ApplicationInformation appInfo)
        {
            if (appInfo == null) throw new ArgumentNullException("appInfo");
            this.appInfo = new Lazy<ApplicationInformation>(()=>appInfo);
        }
        public SmsMessageFormatter(Lazy<ApplicationInformation> appInfo)
        {
            if (appInfo == null) throw new ArgumentNullException("appInfo");
            this.appInfo = appInfo;
        }
        public ApplicationInformation ApplicationInformation
        {
            get
            {
                return appInfo.Value;
            }
        }

        public Message Format(UserAccountEvent accountEvent)
        {
            if (accountEvent == null) throw new ArgumentNullException("accountEvent");

            var message = GetMessageBody(accountEvent);
            return new Message
            {
                Subject = message,
                Body = message
            };
        }

        private string GetMessageBody(UserAccountEvent accountEvent)
        {
            var txt = LoadTemplate();
            
            txt = txt.Replace("{applicationName}", ApplicationInformation.ApplicationName);
            txt = txt.Replace("{code}", accountEvent.Account.MobileCode);

            return txt;
        }

        const string ResourcePathTemplate = "BrockAllen.MembershipReboot.Notification.SMS.SmsTemplates.Code.txt";
        string LoadTemplate()
        {
            var asm = typeof(SmsMessageFormatter).Assembly;
            using (var s = asm.GetManifestResourceStream(ResourcePathTemplate))
            {
                if (s == null) return null;
                using (var sr = new StreamReader(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
