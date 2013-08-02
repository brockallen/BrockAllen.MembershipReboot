/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

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

    public class SuccessfulLoginEvent : UserAccountEvent { }

    public abstract class FailedLoginEvent : UserAccountEvent { }
    public class AccountNotVerifiedEvent : FailedLoginEvent { }
    public class AccountLockedEvent : FailedLoginEvent { }
    public class TooManyRecentPasswordFailuresEvent : FailedLoginEvent { }
    public class InvalidPasswordEvent : FailedLoginEvent { }
}
