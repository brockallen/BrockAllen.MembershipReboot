using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class NotificationService
    {
        IMessageDelivery messageDelivery;
        ApplicationInformation appInfo;
        public NotificationService(IMessageDelivery messageDelivery, ApplicationInformation appInfo)
        {
            this.messageDelivery = messageDelivery;
            this.appInfo = appInfo;
        }

        protected virtual string DoTokenReplacement(string msg, UserAccount user)
        {
            var sb = new StringBuilder();
            sb.Append(msg.Replace("{username}", user.Username));
            sb.Append(msg.Replace("{email}", user.Email));
            
            sb.Append(msg.Replace("{applicationName}", appInfo.ApplicationName));
            sb.Append(msg.Replace("{loginUrl}", appInfo.LoginUrl));
            
            sb.Append(msg.Replace("{confirmAccountCreateUrl}", appInfo.VerifyAccountUrl));
            sb.Append(msg.Replace("{cancelNewAccountUrl}", appInfo.CancelNewAccountUrl));
            
            sb.Append(msg.Replace("{confirmPasswordResetUrl}", appInfo.ConfirmPasswordResetUrl));

            return sb.ToString();
        }
        
        protected virtual void DeliverMessage(UserAccount user, string subject, string body)
        {
            subject = String.Format("[{0}] {1}",
                appInfo.ApplicationName,
                subject);

            var from = this.appInfo.FromEmail;

            var msg = new Message
            {
                From = from,
                To = user.Email, 
                Subject = subject,
                Body = body
            };
            this.messageDelivery.Send(msg);
        }

        public void SendAccountCreate(UserAccount user)
        {
            var msg = GetAccountCreateFormat();
            var body = DoTokenReplacement(msg, user);
            DeliverMessage(user, "Account Created", body);
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
";
        }

        public void SendAccountVerified(UserAccount user)
        {
            var msg = GetAccountVerifiedFormat();
            var body = DoTokenReplacement(msg, user);
            DeliverMessage(user, "Account Verified", body);
        }

        protected virtual string GetAccountVerifiedFormat()
        {
            return @"
Your account has been verified with {applicationName}.

Username: {username}

You can now login at this address:

{loginUrl}

Thanks!
";
        }

        public void SendResetPassword(UserAccount user)
        {
            var msg = GetResetPasswordFormat();
            var body = DoTokenReplacement(msg, user);
            DeliverMessage(user, "Password Reset Request", body);
        }

        protected virtual string GetResetPasswordFormat()
        {
            return @"
You (or someone else) has requested a password reset for {applicationName}.

Username: {username}

Please click here to confirm your request so you can login: 

{confirmPasswordResetUrl}

Thanks!
";
        }

        public void SendPasswordChangeNotice(UserAccount user)
        {
            var msg = GetPasswordChangeNoticeFormat();
            var body = DoTokenReplacement(msg, user);
            DeliverMessage(user, "Password Changed", body);
        }

        protected virtual string GetPasswordChangeNoticeFormat()
        {
            return @"
Your password has been changed at {applicationName}.

Username: {username}

Click here to login:

{loginUrl}

Thanks!
";
        }

        public void SendAccountNameReminder(UserAccount user)
        {
            var msg = GetAccountNameReminderFormat();
            var body = DoTokenReplacement(msg, user);
            DeliverMessage(user, "Username Reminder", body);
        }

        protected virtual string GetAccountNameReminderFormat()
        {
            return @"
You (or someone else) requested a reminder for your username from {applicationName}.

Username: {username}

You can click here to login:

{loginUrl}

Thanks!
";
        }

        public void SendAccountDelete(UserAccount account)
        {
        }
    }
}
