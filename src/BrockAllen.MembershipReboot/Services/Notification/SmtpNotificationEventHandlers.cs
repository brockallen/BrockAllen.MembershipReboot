using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class SmtpNotificationEventHandlers
    {
        public class AccountCreatedEventHandler
            : IEventHandler<UserAccountEvents.AccountCreated>
        {
            public void Handle(UserAccountEvents.AccountCreated evt)
            {
            }
        }

        public class GeneralAccountRelatedEventHandler : 
            IEventHandler<UserAccountEvents.AccountVerified>,
            IEventHandler<UserAccountEvents.PasswordResetRequested>,
            IEventHandler<UserAccountEvents.PasswordChanged>,
            IEventHandler<UserAccountEvents.UsernameReminderRequested>,
            IEventHandler<UserAccountEvents.AccountClosed>,
            IEventHandler<UserAccountEvents.UsernameChanged>,
            IEventHandler<UserAccountEvents.EmailChangeRequested>,
            IEventHandler<UserAccountEvents.EmailChanged>
        {
            public void Handle(UserAccountEvents.AccountVerified evt)
            {
            }

            public void Handle(UserAccountEvents.PasswordResetRequested evt)
            {
            }

            public void Handle(UserAccountEvents.PasswordChanged evt)
            {
            }

            public void Handle(UserAccountEvents.UsernameReminderRequested evt)
            {
            }

            public void Handle(UserAccountEvents.AccountClosed evt)
            {
            }

            public void Handle(UserAccountEvents.UsernameChanged evt)
            {
            }

            public void Handle(UserAccountEvents.EmailChangeRequested evt)
            {
            }

            public void Handle(UserAccountEvents.EmailChanged evt)
            {
            }
        }
    }
}
