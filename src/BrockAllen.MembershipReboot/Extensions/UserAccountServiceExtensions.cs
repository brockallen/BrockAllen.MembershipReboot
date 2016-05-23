/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System;
using System.Collections.Generic;
using System.Security.Claims;
namespace BrockAllen.MembershipReboot
{
    public static class UserAccountServiceExtensions
    {
        public static void ConfigureTwoFactorAuthenticationPolicy<TAccount>(this UserAccountService<TAccount> svc, ITwoFactorAuthenticationPolicy policy)
            where TAccount : UserAccount
        {
            if (svc == null) throw new ArgumentNullException("account");
            
            svc.AddCommandHandler(new TwoFactorAuthPolicyCommandHandler(policy));
        }

        public static void AddClaims<TAccount>(this UserAccountService<TAccount> svc, Guid accountID, IEnumerable<Claim> claims)
            where TAccount : UserAccount
        {
            if (svc == null) throw new ArgumentNullException("account");

            svc.AddClaims(accountID, new UserClaimCollection(claims));
        }

        public static void RemoveClaims<TAccount>(this UserAccountService<TAccount> svc, Guid accountID, IEnumerable<Claim> claims)
            where TAccount : UserAccount
        {
            if (svc == null) throw new ArgumentNullException("account");

            svc.RemoveClaims(accountID, new UserClaimCollection(claims));
        }
        
        public static void UpdateClaims<TAccount>(this UserAccountService<TAccount> svc, Guid accountID, IEnumerable<Claim> additions = null, IEnumerable<Claim> deletions = null)
            where TAccount : UserAccount
        {
            if (svc == null) throw new ArgumentNullException("account");

            svc.UpdateClaims(accountID, new UserClaimCollection(additions), new UserClaimCollection(deletions));
        }
    }
}
