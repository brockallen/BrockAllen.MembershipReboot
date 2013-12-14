/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public abstract class CookieBasedTwoFactorAuthPolicy<TAccount> :
        ITwoFactorAuthenticationPolicy
        where TAccount : UserAccount
    {
        public CookieBasedTwoFactorAuthPolicy()
        {
            this.PersistentCookieDurationInDays = MembershipRebootConstants.AuthenticationService.DefaultPersistentCookieDays;
        }

        public int PersistentCookieDurationInDays { get; set; }

        protected abstract string GetCookie(string name);
        protected abstract void IssueCookie(string name, string value);
        protected abstract void RemoveCookie(string name);

        public string GetTwoFactorAuthToken(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            return GetCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant);
        }

        public void IssueTwoFactorAuthToken(UserAccount account, string token)
        {
            IssueCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant, token);
        }

        public void ClearTwoFactorAuthToken(UserAccount account)
        {
            RemoveCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant);
        }
    }
    
    public abstract class CookieBasedTwoFactorAuthPolicy : CookieBasedTwoFactorAuthPolicy<UserAccount>
    {
    }
}
