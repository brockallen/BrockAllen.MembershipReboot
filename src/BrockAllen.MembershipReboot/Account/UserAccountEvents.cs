/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Security.Cryptography.X509Certificates;
namespace BrockAllen.MembershipReboot
{
    interface IAllowMultiple { }

    public abstract class UserAccountEvent : IEvent
    {
        public UserAccount Account { get; set; }
    }

    public class AccountCreatedEvent : UserAccountEvent
    {
        public string VerificationKey { get; set; }
    }
    public class AccountVerifiedEvent : UserAccountEvent { }

    public class PasswordResetFailedEvent : UserAccountEvent { }
    public class PasswordResetRequestedEvent : UserAccountEvent
    {
        public string VerificationKey { get; set; }
    }
    public class PasswordResetSecretAddedEvent : UserAccountEvent
    {
        public PasswordResetSecret Secret { get; set; }
    }
    public class PasswordResetSecretRemovedEvent : UserAccountEvent
    {
        public PasswordResetSecret Secret { get; set; }
    }
    
    
    public class PasswordChangedEvent : UserAccountEvent
    {
        public string NewPassword { get; set; }
    }
    public class CertificateAddedEvent : UserAccountEvent, IAllowMultiple
    {
        public UserCertificate Certificate { get; set; }
    }
    public class CertificateRemovedEvent : UserAccountEvent, IAllowMultiple
    {
        public UserCertificate Certificate { get; set; }
    }

    public class UsernameReminderRequestedEvent : UserAccountEvent { }
    public class AccountClosedEvent : UserAccountEvent { }
    public class UsernameChangedEvent : UserAccountEvent { }
    public class EmailChangeRequestedEvent : UserAccountEvent
    {
        public string NewEmail { get; set; }
        public string VerificationKey { get; set; }
    }
    public class EmailChangedEvent : UserAccountEvent
    {
        public string OldEmail { get; set; }
    }

    public class MobilePhoneChangeRequestedEvent : UserAccountEvent
    {
        public string NewMobilePhoneNumber { get; set; }
        public string Code { get; set; }
    }
    public class MobilePhoneChangedEvent : UserAccountEvent { }
    public class MobilePhoneRemovedEvent : UserAccountEvent { }

    public class TwoFactorAuthenticationEnabledEvent : UserAccountEvent
    {
        public TwoFactorAuthMode Mode { get; set; }
    }
    public class TwoFactorAuthenticationDisabledEvent : UserAccountEvent { }

    public class TwoFactorAuthenticationCodeNotificationEvent : UserAccountEvent
    {
        public string Code { get; set; }
    }
    public class TwoFactorAuthenticationTokenCreatedEvent : UserAccountEvent
    {
        public string Token { get; set; }
    }

    public class ClaimAddedEvent : UserAccountEvent, IAllowMultiple
    {
        public UserClaim Claim { get; set; }
    }
    public class ClaimRemovedEvent : UserAccountEvent, IAllowMultiple
    {
        public UserClaim Claim { get; set; }
    }

    public class LinkedAccountAddedEvent : UserAccountEvent, IAllowMultiple
    {
        public LinkedAccount LinkedAccount { get; set; }
    }
    public class LinkedAccountRemovedEvent : UserAccountEvent, IAllowMultiple
    {
        public LinkedAccount LinkedAccount { get; set; }
    }

    public abstract class SuccessfulLoginEvent : UserAccountEvent { }
    public class SuccessfulPasswordLoginEvent : SuccessfulLoginEvent { }
    public class SuccessfulCertificateLoginEvent : SuccessfulLoginEvent
    {
        public UserCertificate UserCertificate { get; set; }
        public X509Certificate2 Certificate { get; set; }
    }
    public class SuccessfulTwoFactorAuthCodeLoginEvent : SuccessfulLoginEvent { }

    public abstract class FailedLoginEvent : UserAccountEvent { }
    public class AccountNotVerifiedEvent : FailedLoginEvent { }
    public class AccountLockedEvent : FailedLoginEvent { }
    public class TooManyRecentPasswordFailuresEvent : FailedLoginEvent { }
    public class InvalidPasswordEvent : FailedLoginEvent { }
    public class InvalidCertificateEvent : FailedLoginEvent
    {
        public X509Certificate2 Certificate { get; set; }
    }
}
