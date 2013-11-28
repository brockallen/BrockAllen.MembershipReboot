/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.IO;

namespace BrockAllen.MembershipReboot
{
    public class EmailMessageFormatter<TAccount> : IMessageFormatter<TAccount>
        where TAccount: UserAccount
    {
        public class Tokenizer
        {
            public virtual string Tokenize(
                UserAccountEvent<TAccount> accountEvent, 
                ApplicationInformation appInfo, 
                string msg, 
                IDictionary<string, string> values)
            {
                var user = accountEvent.Account;

                msg = msg.Replace("{username}", user.Username);
                msg = msg.Replace("{email}", user.Email);
                msg = msg.Replace("{mobile}", user.MobilePhoneNumber);

                msg = msg.Replace("{applicationName}", appInfo.ApplicationName);
                msg = msg.Replace("{emailSignature}", appInfo.EmailSignature);
                msg = msg.Replace("{loginUrl}", appInfo.LoginUrl);

                if (values.ContainsKey("VerificationKey"))
                {
                    msg = msg.Replace("{confirmPasswordResetUrl}", appInfo.ConfirmPasswordResetUrl + values["VerificationKey"]);
                    msg = msg.Replace("{confirmChangeEmailUrl}", appInfo.ConfirmChangeEmailUrl + values["VerificationKey"]);
                    msg = msg.Replace("{cancelVerificationUrl}", appInfo.CancelVerificationUrl + values["VerificationKey"]);
                }

                foreach(var item in values)
                {
                    msg = msg.Replace("{" + item.Key + "}", item.Value);
                }

                return msg;
            }
        }
       
        public ApplicationInformation ApplicationInformation 
        {
            get
            {
                return appInfo.Value;
            }
        }

        Lazy<ApplicationInformation> appInfo;
        public EmailMessageFormatter(ApplicationInformation appInfo)
        {
            if (appInfo == null) throw new ArgumentNullException("appInfo");
            this.appInfo = new Lazy<ApplicationInformation>(()=>appInfo);
        }
        public EmailMessageFormatter(Lazy<ApplicationInformation> appInfo)
        {
            if (appInfo == null) throw new ArgumentNullException("appInfo");
            this.appInfo = appInfo;
        }

        public Message Format(UserAccountEvent<TAccount> accountEvent, IDictionary<string, string> values)
        {
            if (accountEvent == null) throw new ArgumentNullException("accountEvent");
            return CreateMessage(GetSubject(accountEvent, values), GetBody(accountEvent, values));
        }

        protected virtual Tokenizer GetTokenizer(UserAccountEvent<TAccount> evt)
        {
            return new Tokenizer();
        }

        protected Message CreateMessage(string subject, string body)
        {
            if (subject == null || body == null) return null;

            return new Message { Subject = subject, Body = body };
        }

        protected string FormatValue(UserAccountEvent<TAccount> evt, string value, IDictionary<string, string> values)
        {
            if (value == null) return null;

            var tokenizer = GetTokenizer(evt);
            return tokenizer.Tokenize(evt, this.ApplicationInformation, value, values);
        }

        protected virtual string GetSubject(UserAccountEvent<TAccount> evt, IDictionary<string, string> values)
        {
            return FormatValue(evt, LoadSubjectTemplate(evt), values);
        }
        protected virtual string GetBody(UserAccountEvent<TAccount> evt, IDictionary<string, string> values)
        {
            return FormatValue(evt, LoadBodyTemplate(evt), values);
        }

        protected virtual string LoadSubjectTemplate(UserAccountEvent<TAccount> evt)
        {
            return LoadTemplate(CleanGenericName(evt.GetType()) + "_Subject");
        }
        protected virtual string LoadBodyTemplate(UserAccountEvent<TAccount> evt)
        {
            return LoadTemplate(CleanGenericName(evt.GetType()) + "_Body");
        }

        private string CleanGenericName(Type type)
        {
            var name = type.Name;
            var idx = name.IndexOf('`');
            if (idx > 0)
            {
                name = name.Substring(0, idx);
            }
            return name;
        }

        const string ResourcePathTemplate = "BrockAllen.MembershipReboot.Notification.Email.EmailTemplates.{0}.txt";
        string LoadTemplate(string name)
        {
            name = String.Format(ResourcePathTemplate, name);

            var asm = typeof(EmailMessageFormatter<>).Assembly;
            using (var s = asm.GetManifestResourceStream(name))
            {
                if (s == null) return null;
                using (var sr = new StreamReader(s))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }

    public class EmailMessageFormatter : EmailMessageFormatter<UserAccount>
    {
        public EmailMessageFormatter(ApplicationInformation appInfo)
            : base(appInfo)
        {
        }
        public EmailMessageFormatter(Lazy<ApplicationInformation> appInfo)
            : base(appInfo)
        {
        }
    }
}
