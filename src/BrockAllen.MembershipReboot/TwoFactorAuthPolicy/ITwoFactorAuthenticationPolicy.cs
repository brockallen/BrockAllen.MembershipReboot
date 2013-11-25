/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public interface ITwoFactorAuthenticationPolicy
    {
        string GetTwoFactorAuthToken(UserAccount account);
    }
}
