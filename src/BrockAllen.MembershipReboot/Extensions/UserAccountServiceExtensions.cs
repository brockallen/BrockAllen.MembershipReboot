/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
