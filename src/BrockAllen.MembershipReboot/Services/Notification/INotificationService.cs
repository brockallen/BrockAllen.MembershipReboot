
namespace BrockAllen.MembershipReboot
{
    //public interface INotificationService : INotificationService<int>
    //{
    //}

    public interface INotificationService
    {
        void SendAccountCreate<TKey>(UserAccount<TKey> user);
        void SendAccountVerified<TKey>(UserAccount<TKey> user);
        void SendResetPassword<TKey>(UserAccount<TKey> user);
        void SendPasswordChangeNotice<TKey>(UserAccount<TKey> user);
        void SendAccountNameReminder<TKey>(UserAccount<TKey> user);
        void SendAccountDelete<TKey>(UserAccount<TKey> user);
        void SendChangeUsernameRequestNotice<TKey>(UserAccount<TKey> user);
        void SendChangeEmailRequestNotice<TKey>(UserAccount<TKey> user, string newEmail);
        void SendEmailChangedNotice<TKey>(UserAccount<TKey> user, string oldEmail);
    }
}
