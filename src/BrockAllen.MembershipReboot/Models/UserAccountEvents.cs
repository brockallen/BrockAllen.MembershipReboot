/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Security.Cryptography.X509Certificates;
namespace BrockAllen.MembershipReboot
{
    public abstract class UserAccountEvent : IEvent
    {
        public UserAccount Account { get; set; }
    }

    public class AccountCreatedEvent : UserAccountEvent { }
    public class AccountVerifiedEvent : UserAccountEvent { }

    public class PasswordResetRequestedEvent : UserAccountEvent { }
    public class PasswordChangedEvent : UserAccountEvent { }
    public class CertificateChangedEvent : UserAccountEvent { }
    public class UsernameReminderRequestedEvent : UserAccountEvent { }
    public class AccountClosedEvent : UserAccountEvent { }
    public class UsernameChangedEvent : UserAccountEvent { }
    public class EmailChangeRequestedEvent : UserAccountEvent
    {
        public string NewEmail { get; set; }
    }
    public class EmailChangedEvent : UserAccountEvent
    {
        public string OldEmail { get; set; }
    }

    public class MobilePhoneChangeRequestedEvent : UserAccountEvent
    {
        public string NewMobilePhoneNumber { get; set; }
    }
    public class MobilePhoneChangedEvent : UserAccountEvent { }
    public class MobilePhoneRemovedEvent : UserAccountEvent { }
    
    public class TwoFactorAuthenticationEnabledEvent : UserAccountEvent { }
    public class TwoFactorAuthenticationDisabledEvent : UserAccountEvent { }

    public class TwoFactorAuthenticationCodeNotificationEvent : UserAccountEvent { }

    public abstract class SuccessfulLoginEvent : UserAccountEvent { }
    public class SuccessfulPasswordLoginEvent : SuccessfulLoginEvent { }
    public class SuccessfulCertificateLoginEvent : SuccessfulLoginEvent {
        public UserCertificate UserCertificate { get; set; }
        public X509Certificate2 Certificate { get; set; }
    }
    public class SuccessfulTwoFactorAuthCodeLoginEvent : SuccessfulLoginEvent { }

    public abstract class FailedLoginEvent : UserAccountEvent { }
    public class AccountNotVerifiedEvent : FailedLoginEvent { }
    public class AccountLockedEvent : FailedLoginEvent { }
    public class TooManyRecentPasswordFailuresEvent : FailedLoginEvent { }
    public class InvalidPasswordEvent : FailedLoginEvent { }
    public class InvalidCertificateEvent : FailedLoginEvent {
        public X509Certificate2 Certificate { get; set; }
    }
}
