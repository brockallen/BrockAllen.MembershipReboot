namespace BrockAllen.MembershipReboot
{
    /// <summary>
    /// The various reasons why a call to <code>UserAccountService.Authenticate</code> will return false
    /// </summary>
    /// <remarks>
    /// Note to maintainers: <see cref="AuthenticationFailureCode"/> symbol names should have a matching
    /// constant declared in <see cref="MembershipRebootConstants.ValidationMessages"/>
    /// </remarks>
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