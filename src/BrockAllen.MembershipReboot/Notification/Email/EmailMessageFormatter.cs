using System;
using System.Collections.Generic;
using System.Linq;
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

            var type = accountEvent.GetType();
            var msg = GetMessageFormat(type);
            if (msg != null)
            {
                var tokenizer = GetTokenizer(type);
                msg.Body = tokenizer.Tokenize(accountEvent, appInfo.Value, msg.Body);
                msg.Subject = FormatSubject(accountEvent, appInfo.Value, msg.Subject);
            }

            return msg;
        }

        protected virtual string FormatSubject(UserAccountEvent accountEvent, ApplicationInformation appInfo, string subject)
        {
            return String.Format("[{0}] {1}", appInfo.ApplicationName, subject);
        }

        protected Tokenizer GetTokenizer(Type type)
        {
            if (type == typeof(EmailChangeRequestedEvent)) return new EmailChangeRequestedTokenizer();
            if (type == typeof(EmailChangedEvent)) return new EmailChangedTokenizer();
            return new Tokenizer();
        }

        protected virtual Message GetMessageFormat(Type type)
        {
            if (type == typeof(AccountCreatedEvent)) return new Message { Body = GetAccountCreateFormat(), Subject = "Account Created" };
            if (type == typeof(AccountVerifiedEvent)) return new Message { Body = GetAccountVerifiedFormat(), Subject = "Account Verified" };
            if (type == typeof(PasswordResetRequestedEvent)) return new Message { Body = GetResetPasswordFormat(), Subject = "Password Reset Request" };
            if (type == typeof(PasswordChangedEvent)) return new Message { Body = GetPasswordChangeNoticeFormat(), Subject = "Password Changed" };
            if (type == typeof(UsernameReminderRequestedEvent)) return new Message { Body = GetAccountNameReminderFormat(), Subject = "Username Reminder" };
            if (type == typeof(AccountClosedEvent)) return new Message { Body = GetAccountClosedFormat(), Subject = "Account Closed" };
            if (type == typeof(EmailChangeRequestedEvent)) return new Message { Body = GetChangeEmailRequestNoticeFormat(), Subject = "Change Email Request" };
            if (type == typeof(EmailChangedEvent)) return new Message { Body = GetEmailChangedNoticeFormat(), Subject = "Email Changed" };
            if (type == typeof(UsernameChangedEvent)) return new Message { Body = GetUsernameChangedNoticeFormat(), Subject = "Username Changed" };

            if (type == typeof(AccountLockedEvent)) return null;
            if (type == typeof(AccountNotVerifiedEvent)) return null;
            if (type == typeof(FailedLoginEvent)) return null;
            if (type == typeof(InvalidPasswordEvent)) return null;
            if (type == typeof(SuccessfulLoginEvent)) return null;
            if (type == typeof(TooManyRecentPasswordFailuresEvent)) return null;
            
            return null;
        }

        protected virtual string GetAccountCreateFormat()
        {
            return @"
You (or someone) requested an account to be created with {applicationName}.

Username: {username}

Please click here to confirm your request so you can login: 

{confirmAccountCreateUrl}

If you did not create this account click here to cancel this request:

{cancelNewAccountUrl}

Thanks!

{emailSignature}
";
        }

        protected virtual string GetAccountVerifiedFormat()
        {
            return @"
Your account has been verified with {applicationName}.

Username: {username}

You can now login at this address:

{loginUrl}

Thanks!

{emailSignature}
";
        }

        protected virtual string GetResetPasswordFormat()
        {
            return @"
You (or someone else) has requested a password reset for {applicationName}.

Username: {username}

Please click here to confirm your request so you can reset your password: 

{confirmPasswordResetUrl}

Thanks!

{emailSignature}
";
        }

        protected virtual string GetPasswordChangeNoticeFormat()
        {
            return @"
Your password has been changed at {applicationName}.

Username: {username}

Click here to login:

{loginUrl}

Thanks!

{emailSignature}
";
        }
        protected virtual string GetAccountNameReminderFormat()
        {
            return @"
You (or someone else) requested a reminder for your username from {applicationName}.

Username: {username}

You can click here to login:

{loginUrl}

Thanks!

{emailSignature}
";
        }
        protected virtual string GetAccountClosedFormat()
        {
            return @"
This email is to confirm that the account '{username}' has been closed for {applicationName}.

Thanks!

{emailSignature}
";
        }
        protected virtual string GetChangeEmailRequestNoticeFormat()
        {
            return @"
This email is to confirm an email change for your account with {applicationName}.

Username: {username}
Old Email: {oldEmail}
New Email: {newEmail}

Please click here to confirm your email change request:

{confirmChangeEmailUrl}

Thanks!

{emailSignature}
";
        }
        protected virtual string GetEmailChangedNoticeFormat()
        {
            return @"
This email is to confirm that your email has been changed for your account with {applicationName}.

Username: {username}
New Email: {newEmail}

Thanks!

{emailSignature}
";
        }
        protected virtual string GetUsernameChangedNoticeFormat()
        {
            return @"
This email is to confirm that your username has been changed for your account with {applicationName}.

New Username: {username}

Thanks!

{emailSignature}
";
        }
    }
}
