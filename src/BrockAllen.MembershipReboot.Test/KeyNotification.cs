using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test
{
    public class KeyNotification :
             IEventHandler<AccountCreatedEvent<UserAccount>>,
             IEventHandler<PasswordResetRequestedEvent<UserAccount>>,
             IEventHandler<EmailChangeRequestedEvent<UserAccount>>,
             IEventHandler<MobilePhoneChangeRequestedEvent<UserAccount>>,
             IEventHandler<TwoFactorAuthenticationCodeNotificationEvent<UserAccount>>
    {
        public string LastVerificationKey { get; set; }
        public string LastMobileCode { get; set; }
        public IEvent LastEvent { get; set; }
        
        public void Handle(PasswordResetRequestedEvent<UserAccount> evt)
        {
            LastEvent = evt;
            LastVerificationKey = evt.VerificationKey;
        }

        public void Handle(EmailChangeRequestedEvent<UserAccount> evt)
        {
            LastEvent = evt;
            LastVerificationKey = evt.VerificationKey;
        }

        public void Handle(MobilePhoneChangeRequestedEvent<UserAccount> evt)
        {
            LastEvent = evt;
            LastMobileCode = evt.Code;
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent<UserAccount> evt)
        {
            LastEvent = evt;
            LastMobileCode = evt.Code;
        }

        public void Handle(AccountCreatedEvent<UserAccount> evt)
        {
            LastEvent = evt;
            LastVerificationKey = evt.VerificationKey;
        }
    }
}
