/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public interface ITwoFactorAuthenticationPolicy
    {
        string GetTwoFactorAuthToken(string prefix);
        void IssueTwoFactorAuthToken(string prefix, string token);
        void RemoveTwoFactorAuthToken(string prefix);
    }
}
