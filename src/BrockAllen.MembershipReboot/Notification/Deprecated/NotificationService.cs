/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class NotificationService : INotificationService
    {
        ApplicationInformation appInfo;
        EmailEventHandler smtpEventHandler;
        
        public NotificationService(IMessageDelivery messageDelivery, ApplicationInformation appInfo)
        {
            this.smtpEventHandler = new EmailEventHandler(new EmailMessageFormatter(appInfo), messageDelivery);
            this.appInfo = appInfo;
        }

        public void SendAccountCreate(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendAccountCreate] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new AccountCreatedEvent { Account = user });
        }

        public void SendAccountVerified(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendAccountVerified] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new AccountVerifiedEvent { Account = user });
        }

        public void SendResetPassword(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendResetPassword] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new PasswordResetRequestedEvent { Account = user });
        }

        public void SendPasswordChangeNotice(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendPasswordChangeNotice] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new PasswordChangedEvent { Account = user });
        }

        public void SendAccountNameReminder(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendAccountNameReminder] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new UsernameReminderRequestedEvent { Account = user });
        }

        public void SendAccountDelete(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendAccountDelete] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new AccountClosedEvent { Account = user });
        }

        public void SendChangeEmailRequestNotice(UserAccount user, string newEmail)
        {
            Tracing.Information(String.Format("[NotificationService.SendChangeEmailRequestNotice] {0}, {1}, {2}, {3}", user.Tenant, user.Username, user.Email, newEmail));
            smtpEventHandler.Process(new EmailChangeRequestedEvent { Account = user, NewEmail=newEmail  }, newEmail);
        }

        public void SendEmailChangedNotice(UserAccount user, string oldEmail)
        {
            Tracing.Information(String.Format("[NotificationService.SendEmailChangedNotice] {0}, {1}, {2}, {3}", user.Tenant, user.Username, user.Email, oldEmail));
            smtpEventHandler.Process(new EmailChangedEvent { Account = user });
        }

        public void SendChangeUsernameRequestNotice(UserAccount user)
        {
            Tracing.Information(String.Format("[NotificationService.SendChangeUsernameRequestNotice] {0}, {1}, {2}", user.Tenant, user.Username, user.Email));
            smtpEventHandler.Process(new UsernameChangedEvent { Account = user });
        }

    }
}
