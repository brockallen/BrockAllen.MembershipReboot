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
            msg = msg.Replace("{username}", user.Username);
            msg = msg.Replace("{email}", user.Email);

            msg = msg.Replace("{applicationName}", appInfo.ApplicationName);
            msg = msg.Replace("{loginUrl}", appInfo.LoginUrl);

            msg = msg.Replace("{confirmAccountCreateUrl}", appInfo.VerifyAccountUrl + user.VerificationKey);
            msg = msg.Replace("{cancelNewAccountUrl}", appInfo.CancelNewAccountUrl + user.VerificationKey);

            msg = msg.Replace("{confirmPasswordResetUrl}", appInfo.ConfirmPasswordResetUrl + user.VerificationKey);

            return msg; 
        }
        
        protected virtual void DeliverMessage(UserAccount user, string subject, string body)
        {
            subject = String.Format("[{0}] {1}",
                appInfo.ApplicationName,
                subject);

            var msg = new Message
            {
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

Please click here to confirm your request so you can reset your password: 

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
            var msg = GetAccountAccountDeleteFormat();
            var body = DoTokenReplacement(msg, account);
            DeliverMessage(account, "Account Closed", body);
        }

        protected virtual string GetAccountAccountDeleteFormat()
        {
            return @"
This email is to confirm that your account has been closed for {applicationName}.

Thanks!
";
        }
    }
}
