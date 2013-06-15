using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class NotificationServiceAccountCreatedEventHandler :
        IEventHandler<UserAccountEvents.AccountCreated>
    {
        INotificationService notificationService;
        public NotificationServiceAccountCreatedEventHandler(INotificationService notificationService)
        {
            if (notificationService == null) throw new ArgumentNullException("notificationService");

            this.notificationService = notificationService;
        }

        public void Handle(UserAccountEvents.AccountCreated evt)
        {
            notificationService.SendAccountCreate(evt.Account);
        }
    }

    public class NotificationServiceEventHandler :
        IEventHandler<UserAccountEvents.AccountVerified>,
        IEventHandler<UserAccountEvents.PasswordResetRequested>,
        IEventHandler<UserAccountEvents.PasswordChanged>,
        IEventHandler<UserAccountEvents.UsernameReminderRequested>,
        IEventHandler<UserAccountEvents.AccountClosed>,
        IEventHandler<UserAccountEvents.UsernameChanged>,
        IEventHandler<UserAccountEvents.EmailChangeRequested>,
        IEventHandler<UserAccountEvents.EmailChanged>
    {
        INotificationService notificationService;
        public NotificationServiceEventHandler(INotificationService notificationService)
        {
            if (notificationService == null) throw new ArgumentNullException("notificationService");
            
            this.notificationService = notificationService;
        }

        public void Handle(UserAccountEvents.AccountCreated evt)
        {
            notificationService.SendAccountCreate(evt.Account);
        }

        public void Handle(UserAccountEvents.AccountVerified evt)
        {
            notificationService.SendAccountVerified(evt.Account);
        }

        public void Handle(UserAccountEvents.PasswordResetRequested evt)
        {
            notificationService.SendResetPassword(evt.Account);
        }

        public void Handle(UserAccountEvents.PasswordChanged evt)
        {
            notificationService.SendPasswordChangeNotice(evt.Account);
        }

        public void Handle(UserAccountEvents.UsernameReminderRequested evt)
        {
            notificationService.SendAccountNameReminder(evt.Account);
        }

        public void Handle(UserAccountEvents.AccountClosed evt)
        {
            notificationService.SendAccountDelete(evt.Account);
        }

        public void Handle(UserAccountEvents.UsernameChanged evt)
        {
            notificationService.SendChangeUsernameRequestNotice(evt.Account);
        }

        public void Handle(UserAccountEvents.EmailChangeRequested evt)
        {
            notificationService.SendChangeEmailRequestNotice(evt.Account, evt.NewEmail);
        }

        public void Handle(UserAccountEvents.EmailChanged evt)
        {
            notificationService.SendEmailChangedNotice(evt.Account, evt.OldEmail);
        }
    }
}
