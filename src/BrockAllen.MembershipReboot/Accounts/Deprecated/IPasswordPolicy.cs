/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot
{
    public interface IPasswordPolicy
    {
        string PolicyMessage { get; }
        bool ValidatePassword(string password);
    }
}
