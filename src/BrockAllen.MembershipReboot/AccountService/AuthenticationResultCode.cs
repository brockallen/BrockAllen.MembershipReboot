namespace BrockAllen.MembershipReboot
{
    public enum AuthenticationFailureCode
    {
        None,
        AccountClosed,
        AccountNotConfiguredWithCertificates,
        AccountNotConfiguredWithMobilePhone,
        AccountNotVerified,
        FailedLoginAttemptsExceeded,
        InvalidCredentials,
        LoginNotAllowed
    }
}