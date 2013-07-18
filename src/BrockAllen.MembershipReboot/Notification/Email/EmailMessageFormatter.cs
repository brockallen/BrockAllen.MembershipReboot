/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EmailMessageFormatter : IMessageFormatter
    {
        public class Tokenizer
        {
            public virtual string Tokenize(UserAccountEvent accountEvent, ApplicationInformation appInfo, string msg)
            {
                var user = accountEvent.Account;

                msg = msg.Replace("{username}", user.Username);
                msg = msg.Replace("{email}", user.Email);

                msg = msg.Replace("{applicationName}", appInfo.ApplicationName);
                msg = msg.Replace("{emailSignature}", appInfo.EmailSignature);
                msg = msg.Replace("{loginUrl}", appInfo.LoginUrl);

                msg = msg.Replace("{confirmAccountCreateUrl}", appInfo.VerifyAccountUrl + user.VerificationKey);
                msg = msg.Replace("{cancelNewAccountUrl}", appInfo.CancelNewAccountUrl + user.VerificationKey);

                msg = msg.Replace("{confirmPasswordResetUrl}", appInfo.ConfirmPasswordResetUrl + user.VerificationKey);
                msg = msg.Replace("{confirmChangeEmailUrl}", appInfo.ConfirmChangeEmailUrl + user.VerificationKey);

                return msg;
            }
        }
        public class EmailChangeRequestedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent accountEvent, ApplicationInformation appInfo, string msg)
            {
                var evt = (EmailChangeRequestedEvent)accountEvent;
                msg = base.Tokenize(accountEvent, appInfo, msg);
                msg = msg.Replace("{newEmail}", evt.NewEmail);
                msg = msg.Replace("{oldEmail}", accountEvent.Account.Email);
                return msg;
            }
        }
        public class EmailChangedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent accountEvent, ApplicationInformation appInfo, string msg)
            {
                var evt = (EmailChangedEvent)accountEvent;
                msg = base.Tokenize(accountEvent, appInfo, msg);
                msg = msg.Replace("{newEmail}", accountEvent.Account.Email);
                msg = msg.Replace("{oldEmail}", evt.OldEmail);
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

        public Message Format(UserAccountEvent accountEvent)
        {
            if (accountEvent == null) throw new ArgumentNullException("accountEvent");
            return CreateMessage(GetSubject(accountEvent), GetBody(accountEvent));
        }

        protected virtual Tokenizer GetTokenizer(UserAccountEvent evt)
        {
            Type type = evt.GetType();
            if (type == typeof(EmailChangeRequestedEvent)) return new EmailChangeRequestedTokenizer();
            if (type == typeof(EmailChangedEvent)) return new EmailChangedTokenizer();
            return new Tokenizer();
        }

        protected Message CreateMessage(string subject, string body)
        {
            if (subject == null || body == null) return null;

            return new Message { Subject = subject, Body = body };
        }

        protected string FormatValue(UserAccountEvent evt, string value)
        {
            if (value == null) return null;

            var tokenizer = GetTokenizer(evt);
            return tokenizer.Tokenize(evt, this.ApplicationInformation, value);
        }
        
        protected virtual string GetSubject(UserAccountEvent evt)
        {
            return FormatValue(evt, LoadSubjectTemplate(evt));
        }
        protected virtual string GetBody(UserAccountEvent evt)
        {
            return FormatValue(evt, LoadBodyTemplate(evt));
        }

        protected virtual string LoadSubjectTemplate(UserAccountEvent evt)
        {
            return LoadTemplate(evt.GetType().Name + "_Subject");
        }
        protected virtual string LoadBodyTemplate(UserAccountEvent evt)
        {
            return LoadTemplate(evt.GetType().Name + "_Body");
        }

        const string ResourcePathTemplate = "BrockAllen.MembershipReboot.Notification.Email.EmailTemplates.{0}.txt";
        string LoadTemplate(string name)
        {
            name = String.Format(ResourcePathTemplate, name);
            
            var asm = typeof(EmailMessageFormatter).Assembly;
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
}
