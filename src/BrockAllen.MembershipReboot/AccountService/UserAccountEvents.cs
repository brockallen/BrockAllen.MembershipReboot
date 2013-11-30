/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Security.Cryptography.X509Certificates;
namespace BrockAllen.MembershipReboot
{
    interface IAllowMultiple { }

    public abstract class UserAccountEvent<TAccount> : IEvent
    {
        //public UserAccountService<T> UserAccountService { get; set; }
        public TAccount Account { get; set; }
    }

    public class AccountCreatedEvent<T> : UserAccountEvent<T>
    {
        // InitialPassword might be null if this is a re-send
        // notification for account created (when user tries to
        // reset password before verifying their account)
        public string InitialPassword { get; set; }
        public string VerificationKey { get; set; }
    }

    public class PasswordResetFailedEvent<T> : UserAccountEvent<T> { }
    public class PasswordResetRequestedEvent<T> : UserAccountEvent<T>
    {
        public string VerificationKey { get; set; }
    }
    public class PasswordChangedEvent<T> : UserAccountEvent<T>
    {
        public string NewPassword { get; set; }
    }
    public class PasswordResetSecretAddedEvent<T> : UserAccountEvent<T>
    {
        public PasswordResetSecret Secret { get; set; }
    }
    public class PasswordResetSecretRemovedEvent<T> : UserAccountEvent<T>
    {
        public PasswordResetSecret Secret { get; set; }
    }
    
    public class CertificateAddedEvent<T> : UserAccountEvent<T>, IAllowMultiple
    {
        public UserCertificate Certificate { get; set; }
    }
    public class CertificateRemovedEvent<T> : UserAccountEvent<T>, IAllowMultiple
    {
        public UserCertificate Certificate { get; set; }
    }

    public class UsernameReminderRequestedEvent<T> : UserAccountEvent<T> { }
    public class AccountClosedEvent<T> : UserAccountEvent<T> { }
    public class UsernameChangedEvent<T> : UserAccountEvent<T> { }
    
    public class EmailChangeRequestedEvent<T> : UserAccountEvent<T>
    {
        public string OldEmail { get; set; }
        public string NewEmail { get; set; }
        public string VerificationKey { get; set; }
    }
    public class EmailChangedEvent<T> : UserAccountEvent<T>
    {
        public string OldEmail { get; set; }
        public string VerificationKey { get; set; }
    }
    public class EmailVerifiedEvent<T> : UserAccountEvent<T> { }

    public class MobilePhoneChangeRequestedEvent<T> : UserAccountEvent<T>
    {
        public string NewMobilePhoneNumber { get; set; }
        public string Code { get; set; }
    }
    public class MobilePhoneChangedEvent<T> : UserAccountEvent<T> { }
    public class MobilePhoneRemovedEvent<T> : UserAccountEvent<T> { }

    public class TwoFactorAuthenticationEnabledEvent<T> : UserAccountEvent<T>
    {
        public TwoFactorAuthMode Mode { get; set; }
    }
    public class TwoFactorAuthenticationDisabledEvent<T> : UserAccountEvent<T> { }

    public class TwoFactorAuthenticationCodeNotificationEvent<T> : UserAccountEvent<T>
    {
        public string Code { get; set; }
    }
    public class TwoFactorAuthenticationTokenCreatedEvent<T> : UserAccountEvent<T>
    {
        public string Token { get; set; }
    }

    public class ClaimAddedEvent<T> : UserAccountEvent<T>, IAllowMultiple
    {
        public UserClaim Claim { get; set; }
    }
    public class ClaimRemovedEvent<T> : UserAccountEvent<T>, IAllowMultiple
    {
        public UserClaim Claim { get; set; }
    }

    public class LinkedAccountAddedEvent<T> : UserAccountEvent<T>, IAllowMultiple
    {
        public LinkedAccount LinkedAccount { get; set; }
    }
    public class LinkedAccountRemovedEvent<T> : UserAccountEvent<T>, IAllowMultiple
    {
        public LinkedAccount LinkedAccount { get; set; }
    }

    public abstract class SuccessfulLoginEvent<T> : UserAccountEvent<T> { }
    public class SuccessfulPasswordLoginEvent<T> : SuccessfulLoginEvent<T> { }
    public class SuccessfulCertificateLoginEvent<T> : SuccessfulLoginEvent<T>
    {
        public UserCertificate UserCertificate { get; set; }
        public X509Certificate2 Certificate { get; set; }
    }
    public class SuccessfulTwoFactorAuthCodeLoginEvent<T> : SuccessfulLoginEvent<T> { }

    public abstract class FailedLoginEvent<T> : UserAccountEvent<T> { }
    public class AccountNotVerifiedEvent<T> : FailedLoginEvent<T> { }
    public class AccountLockedEvent<T> : FailedLoginEvent<T> { }
    public class InvalidAccountEvent<T> : FailedLoginEvent<T> { }
    public class TooManyRecentPasswordFailuresEvent<T> : FailedLoginEvent<T> { }
    public class InvalidPasswordEvent<T> : FailedLoginEvent<T> { }
    public class InvalidCertificateEvent<T> : FailedLoginEvent<T>
    {
        public X509Certificate2 Certificate { get; set; }
    }
}
