namespace BrockAllen.MembershipReboot
{
    public enum PartialAuthReason
    {
        None,
        PendingTwoFactorAuth,
        PasswordResetRequired,
        PasswordExpired
    }
}