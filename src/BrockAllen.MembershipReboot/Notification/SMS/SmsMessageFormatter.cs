/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace BrockAllen.MembershipReboot
{
    public class SmsMessageFormatter<T> : IMessageFormatter<T>
        where T : UserAccount
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

        public Message Format(UserAccountEvent<T> accountEvent, dynamic extra)
        {
            if (accountEvent == null) throw new ArgumentNullException("accountEvent");

            var message = GetMessageBody(accountEvent, extra);
            return new Message
            {
                Subject = message,
                Body = message
            };
        }

        private string GetMessageBody(UserAccountEvent<T> accountEvent, dynamic extra)
        {
            var txt = LoadTemplate();
            
            txt = txt.Replace("{applicationName}", ApplicationInformation.ApplicationName);
            txt = txt.Replace("{code}", extra.Code);

            return txt;
        }

        const string ResourcePathTemplate = "BrockAllen.MembershipReboot.Notification.SMS.SmsTemplates.Code.txt";
        string LoadTemplate()
        {
            var asm = typeof(SmsMessageFormatter<>).Assembly;
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
    
    public class SmsMessageFormatter : SmsMessageFormatter<UserAccount>
    {
        public SmsMessageFormatter(ApplicationInformation appInfo)
            : base(appInfo)
        {
        }
        public SmsMessageFormatter(Lazy<ApplicationInformation> appInfo)
            : base(appInfo)
        {
        }
    }
}
