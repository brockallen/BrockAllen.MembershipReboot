
namespace BrockAllen.MembershipReboot
{
    public interface INotificationService : INotificationService<int>
    {
    }

    public interface INotificationService<TKey>
    {
        void SendAccountCreate(UserAccount<TKey> user);
        void SendAccountVerified(UserAccount<TKey> user);
        void SendResetPassword(UserAccount<TKey> user);
        void SendPasswordChangeNotice(UserAccount<TKey> user);
        void SendAccountNameReminder(UserAccount<TKey> user);
        void SendAccountDelete(UserAccount<TKey> user);
        void SendChangeUsernameRequestNotice(UserAccount<TKey> user);
        void SendChangeEmailRequestNotice(UserAccount<TKey> user, string newEmail);
        void SendEmailChangedNotice(UserAccount<TKey> user, string oldEmail);
    }
}
