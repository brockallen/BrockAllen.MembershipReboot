/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public static class UserAccountServiceExtensions
    {
        public static void ConfigureTwoFactorAuthenticationPolicy<TAccount>(this UserAccountService<TAccount> svc, ITwoFactorAuthenticationPolicy policy)
            where TAccount : UserAccount
        {
            svc.AddCommandHandler(new TwoFactorAuthPolicyCommandHandler(policy));
        }
    }
}
