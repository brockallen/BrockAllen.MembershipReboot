using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class DebuggerEventHandler<TAccount> : 
        IEventHandler<AccountCreatedEvent<TAccount>>,
        IEventHandler<PasswordResetRequestedEvent<TAccount>>,
        IEventHandler<EmailChangeRequestedEvent<TAccount>>,
        IEventHandler<EmailChangedEvent<TAccount>>,
        IEventHandler<MobilePhoneChangeRequestedEvent<TAccount>>,
        IEventHandler<TwoFactorAuthenticationCodeNotificationEvent<TAccount>>
    {
        public void Handle(AccountCreatedEvent<TAccount> evt)
        {
            Debug.WriteLine("AccountCreatedEvent: " + evt.VerificationKey);
        }

        public void Handle(PasswordResetRequestedEvent<TAccount> evt)
        {
            Debug.WriteLine("PasswordResetRequestedEvent: " + evt.VerificationKey);
        }

        public void Handle(EmailChangeRequestedEvent<TAccount> evt)
        {
            Debug.WriteLine("EmailChangeRequestedEvent: " + evt.VerificationKey);
        }

        public void Handle(EmailChangedEvent<TAccount> evt)
        {
            Debug.WriteLine("EmailChangedEvent: " + evt.VerificationKey);
        }

        public void Handle(MobilePhoneChangeRequestedEvent<TAccount> evt)
        {
            Debug.WriteLine("MobilePhoneChangeRequestedEvent: " + evt.Code);
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent<TAccount> evt)
        {
            Debug.WriteLine("TwoFactorAuthenticationCodeNotificationEvent: " + evt.Code);
        }
    }

    public class DebuggerEventHandler : DebuggerEventHandler<UserAccount> { }
}
