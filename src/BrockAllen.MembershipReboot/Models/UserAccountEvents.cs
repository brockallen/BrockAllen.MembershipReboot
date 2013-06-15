using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class UserAccountEvents
    {
        public abstract class AccountEvent : IEvent
        {
            public UserAccount Account { get; set; }
        }

        public class AccountCreated : AccountEvent { }
        public class AccountVerified : AccountEvent { }
        
        public class PasswordResetRequested : AccountEvent { }
        public class PasswordChanged : AccountEvent { }
        public class UsernameReminderRequested : AccountEvent { }
        public class AccountClosed : AccountEvent { }
        public class UsernameChanged : AccountEvent { }
        public class EmailChangeRequested : AccountEvent {
            public string NewEmail { get; set; }
        }
        public class EmailChanged : AccountEvent {
            public string OldEmail { get; set; }
        }

        public class SuccessfulLogin : AccountEvent { }
        public abstract class FailedLogin : AccountEvent { }
        public class AccountNotVerified : FailedLogin { }
        public class AccountLocked : FailedLogin { }
        public class TooManyRecentPasswordFailures : FailedLogin { }
        public class InvalidPassword : FailedLogin { }
    }
}
