/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace BrockAllen.MembershipReboot
{
    public class SmsMessageFormatter<TAccount> : IMessageFormatter<TAccount>
        where TAccount : UserAccount
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

        public Message Format(UserAccountEvent<TAccount> accountEvent, IDictionary<string, string> values)
        {
            if (accountEvent == null) throw new ArgumentNullException("accountEvent");

            var message = GetMessageBody(accountEvent, values);
            return new Message
            {
                Subject = message,
                Body = message
            };
        }

        private string GetMessageBody(UserAccountEvent<TAccount> accountEvent, IDictionary<string, string> values)
        {
            var txt = LoadTemplate();
            
            txt = txt.Replace("{applicationName}", ApplicationInformation.ApplicationName);
            if (values.ContainsKey("Code"))
            {
                txt = txt.Replace("{code}", values["Code"]);
            }

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
