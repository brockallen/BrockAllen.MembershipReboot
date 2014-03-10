/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public abstract class CookieBasedTwoFactorAuthPolicy : ITwoFactorAuthenticationPolicy
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
            var result = GetCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant);
            Tracing.Information("[CookieBasedTwoFactorAuthPolicy.ClearTwoFactorAuthToken] getting cookie for {0}, {1}, found:{2}", account.Tenant, account.Username, result);
            return result;
        }

        public void IssueTwoFactorAuthToken(UserAccount account, string token)
        {
            if (account == null) throw new ArgumentNullException("account");
            Tracing.Information("[CookieBasedTwoFactorAuthPolicy.ClearTwoFactorAuthToken] issuing cookie for {0}, {1}", account.Tenant, account.Username);
            IssueCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant, token);
        }

        public void ClearTwoFactorAuthToken(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            Tracing.Information("[CookieBasedTwoFactorAuthPolicy.ClearTwoFactorAuthToken] clearning cookie for {0}, {1}", account.Tenant, account.Username);
            RemoveCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant);
        }
    }
}
