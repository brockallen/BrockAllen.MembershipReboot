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
            public virtual string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                var user = accountEvent.Account;

                msg = msg.Replace("{username}", user.Username);
                msg = msg.Replace("{email}", user.Email);
                msg = msg.Replace("{mobile}", user.MobilePhoneNumber);

                msg = msg.Replace("{applicationName}", appInfo.ApplicationName);
                msg = msg.Replace("{emailSignature}", appInfo.EmailSignature);
                msg = msg.Replace("{loginUrl}", appInfo.LoginUrl);

                msg = msg.Replace("{confirmAccountCreateUrl}", appInfo.VerifyAccountUrl + extra.VerificationKey);
                msg = msg.Replace("{cancelNewAccountUrl}", appInfo.CancelNewAccountUrl + extra.VerificationKey);

                msg = msg.Replace("{confirmPasswordResetUrl}", appInfo.ConfirmPasswordResetUrl + extra.VerificationKey);
                msg = msg.Replace("{confirmChangeEmailUrl}", appInfo.ConfirmChangeEmailUrl + extra.VerificationKey);

                return msg;
            }
        }
        public class EmailChangeRequestedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                Func<UserAccountEvent<TAccount>, ApplicationInformation, string, dynamic, string> b = base.Tokenize;
                var evt = (EmailChangeRequestedEvent<TAccount>)accountEvent;
                msg = b(accountEvent, appInfo, msg, extra);
                msg = msg.Replace("{newEmail}", evt.NewEmail);
                msg = msg.Replace("{oldEmail}", accountEvent.Account.Email);
                return msg;
            }
        }
        public class EmailChangedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                Func<UserAccountEvent<TAccount>, ApplicationInformation, string, dynamic, string> b = base.Tokenize;
                var evt = (EmailChangedEvent<TAccount>)accountEvent;
                msg = b(accountEvent, appInfo, msg, extra);
                msg = msg.Replace("{newEmail}", accountEvent.Account.Email);
                msg = msg.Replace("{oldEmail}", evt.OldEmail);
                return msg;
            }
        }
        public class CertificateAddedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                var evt = (CertificateAddedEvent<TAccount>)accountEvent;
                Func<UserAccountEvent<TAccount>, ApplicationInformation, string, dynamic, string> b = base.Tokenize;
                msg = b(accountEvent, appInfo, msg, extra);
                msg = msg.Replace("{thumbprint}", evt.Certificate.Thumbprint);
                msg = msg.Replace("{subject}", evt.Certificate.Subject);
                return msg;
            }
        }
        public class CertificateRemovedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                var evt = (CertificateRemovedEvent<TAccount>)accountEvent;
                Func<UserAccountEvent<TAccount>, ApplicationInformation, string, dynamic, string> b = base.Tokenize;
                msg = b(accountEvent, appInfo, msg, extra);
                msg = msg.Replace("{thumbprint}", evt.Certificate.Thumbprint);
                msg = msg.Replace("{subject}", evt.Certificate.Subject);
                return msg;
            }
        }
        public class LinkedAccountAddedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                Func<UserAccountEvent<TAccount>, ApplicationInformation, string, dynamic, string> b = base.Tokenize;
                var evt = (LinkedAccountAddedEvent<TAccount>)accountEvent;
                msg = b(accountEvent, appInfo, msg, extra);
                msg = msg.Replace("{provider}", evt.LinkedAccount.ProviderName);
                return msg;
            }
        }
        public class LinkedAccountRemovedTokenizer : Tokenizer
        {
            public override string Tokenize(UserAccountEvent<TAccount> accountEvent, ApplicationInformation appInfo, string msg, dynamic extra)
            {
                Func<UserAccountEvent<TAccount>, ApplicationInformation, string, dynamic, string> b = base.Tokenize;
                var evt = (LinkedAccountRemovedEvent<TAccount>)accountEvent;
                msg = b(accountEvent, appInfo, msg, extra);
                msg = msg.Replace("{provider}", evt.LinkedAccount.ProviderName);
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

        public Message Format(UserAccountEvent<TAccount> accountEvent, dynamic extra)
        {
            if (accountEvent == null) throw new ArgumentNullException("accountEvent");
            return CreateMessage(GetSubject(accountEvent, extra), GetBody(accountEvent, extra));
        }

        protected virtual Tokenizer GetTokenizer(UserAccountEvent<TAccount> evt)
        {
            Type type = evt.GetType();
            if (type == typeof(EmailChangeRequestedEvent<TAccount>)) return new EmailChangeRequestedTokenizer();
            if (type == typeof(EmailChangedEvent<TAccount>)) return new EmailChangedTokenizer();
            if (type == typeof(CertificateAddedEvent<TAccount>)) return new CertificateAddedTokenizer();
            if (type == typeof(CertificateRemovedEvent<TAccount>)) return new CertificateRemovedTokenizer();
            if (type == typeof(LinkedAccountAddedEvent<TAccount>)) return new LinkedAccountAddedTokenizer();
            if (type == typeof(LinkedAccountRemovedEvent<TAccount>)) return new LinkedAccountRemovedTokenizer();
            return new Tokenizer();
        }

        protected Message CreateMessage(string subject, string body)
        {
            if (subject == null || body == null) return null;

            return new Message { Subject = subject, Body = body };
        }

        protected string FormatValue(UserAccountEvent<TAccount> evt, string value, dynamic extra)
        {
            if (value == null) return null;

            var tokenizer = GetTokenizer(evt);
            return tokenizer.Tokenize(evt, this.ApplicationInformation, value, extra);
        }

        protected virtual string GetSubject(UserAccountEvent<TAccount> evt, dynamic extra)
        {
            return FormatValue(evt, LoadSubjectTemplate(evt), extra);
        }
        protected virtual string GetBody(UserAccountEvent<TAccount> evt, dynamic extra)
        {
            return FormatValue(evt, LoadBodyTemplate(evt), extra);
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
