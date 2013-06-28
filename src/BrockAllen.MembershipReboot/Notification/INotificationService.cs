/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public interface INotificationService
    {
        void SendAccountCreate(UserAccount user);
        void SendAccountVerified(UserAccount user);
        void SendResetPassword(UserAccount user);
        void SendPasswordChangeNotice(UserAccount user);
        void SendAccountNameReminder(UserAccount user);
        void SendAccountDelete(UserAccount user);
        void SendChangeUsernameRequestNotice(UserAccount user);
        void SendChangeEmailRequestNotice(UserAccount user, string newEmail);
        void SendEmailChangedNotice(UserAccount user, string oldEmail);
    }
}
