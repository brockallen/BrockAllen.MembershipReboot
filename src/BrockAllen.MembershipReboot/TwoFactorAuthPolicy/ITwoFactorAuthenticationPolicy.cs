/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public interface ITwoFactorAuthenticationPolicy
    {
        bool RequestRequiresTwoFactorAuth(UserAccount account);
    }
}
