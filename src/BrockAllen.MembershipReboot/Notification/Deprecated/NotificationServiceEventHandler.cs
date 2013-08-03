/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    internal class NotificationServiceAccountCreatedEventHandler :
        IEventHandler<AccountCreatedEvent>
    {
        INotificationService notificationService;
        public NotificationServiceAccountCreatedEventHandler(INotificationService notificationService)
        {
            if (notificationService == null) throw new ArgumentNullException("notificationService");

            this.notificationService = notificationService;
        }

        public void Handle(AccountCreatedEvent evt)
        {
            notificationService.SendAccountCreate(evt.Account);
        }
    }

    public class NotificationServiceEventHandler :
        IEventHandler<AccountVerifiedEvent>,
        IEventHandler<PasswordResetRequestedEvent>,
        IEventHandler<PasswordChangedEvent>,
        IEventHandler<UsernameReminderRequestedEvent>,
        IEventHandler<AccountClosedEvent>,
        IEventHandler<UsernameChangedEvent>,
        IEventHandler<EmailChangeRequestedEvent>,
        IEventHandler<EmailChangedEvent>
    {
        INotificationService notificationService;
        public NotificationServiceEventHandler(INotificationService notificationService)
        {
            if (notificationService == null) throw new ArgumentNullException("notificationService");
            
            this.notificationService = notificationService;
        }

        public void Handle(AccountCreatedEvent evt)
        {
            notificationService.SendAccountCreate(evt.Account);
        }

        public void Handle(AccountVerifiedEvent evt)
        {
            notificationService.SendAccountVerified(evt.Account);
        }

        public void Handle(PasswordResetRequestedEvent evt)
        {
            notificationService.SendResetPassword(evt.Account);
        }

        public void Handle(PasswordChangedEvent evt)
        {
            notificationService.SendPasswordChangeNotice(evt.Account);
        }

        public void Handle(UsernameReminderRequestedEvent evt)
        {
            notificationService.SendAccountNameReminder(evt.Account);
        }

        public void Handle(AccountClosedEvent evt)
        {
            notificationService.SendAccountDelete(evt.Account);
        }

        public void Handle(UsernameChangedEvent evt)
        {
            notificationService.SendChangeUsernameRequestNotice(evt.Account);
        }

        public void Handle(EmailChangeRequestedEvent evt)
        {
            notificationService.SendChangeEmailRequestNotice(evt.Account, evt.NewEmail);
        }

        public void Handle(EmailChangedEvent evt)
        {
            notificationService.SendEmailChangedNotice(evt.Account, evt.OldEmail);
        }
    }
}
