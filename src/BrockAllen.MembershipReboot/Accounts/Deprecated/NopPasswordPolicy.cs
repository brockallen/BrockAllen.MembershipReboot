/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot
{
    public class NopPasswordPolicy : IPasswordPolicy
    {
        public string PolicyMessage
        {
            get { return "There is no password policy."; }
        }

        public bool ValidatePassword(string password)
        {
            return true;
        }
    }
}
