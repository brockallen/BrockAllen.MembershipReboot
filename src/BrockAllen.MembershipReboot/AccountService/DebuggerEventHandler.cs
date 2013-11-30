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
            Debugger.Log(0, "DebuggerEventHandler", "AccountCreatedEvent: " + evt.VerificationKey);
        }

        public void Handle(PasswordResetRequestedEvent<TAccount> evt)
        {
            Debugger.Log(0, "DebuggerEventHandler", "PasswordResetRequestedEvent: " + evt.VerificationKey);
        }

        public void Handle(EmailChangeRequestedEvent<TAccount> evt)
        {
            Debugger.Log(0, "DebuggerEventHandler", "EmailChangeRequestedEvent: " + evt.VerificationKey);
        }

        public void Handle(EmailChangedEvent<TAccount> evt)
        {
            Debugger.Log(0, "DebuggerEventHandler", "EmailChangedEvent: " + evt.VerificationKey);
        }

        public void Handle(MobilePhoneChangeRequestedEvent<TAccount> evt)
        {
            Debugger.Log(0, "DebuggerEventHandler", "MobilePhoneChangeRequestedEvent: " + evt.Code);
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent<TAccount> evt)
        {
            Debugger.Log(0, "DebuggerEventHandler", "TwoFactorAuthenticationCodeNotificationEvent: " + evt.Code);
        }
    }

    public class DebuggerEventHandler : DebuggerEventHandler<UserAccount> { }
}
