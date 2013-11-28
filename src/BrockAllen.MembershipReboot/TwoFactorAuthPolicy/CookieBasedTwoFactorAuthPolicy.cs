/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public abstract class CookieBasedTwoFactorAuthPolicy<TAccount> :
        ITwoFactorAuthenticationPolicy,
        IEventHandler<TwoFactorAuthenticationTokenCreatedEvent<TAccount>>,
        IEventHandler<TwoFactorAuthenticationDisabledEvent<TAccount>>
        where TAccount: UserAccount
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

        public void Handle(TwoFactorAuthenticationTokenCreatedEvent<TAccount> evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");
            if (evt.Token == null) throw new ArgumentNullException("Token");
            if (evt.Account == null) throw new ArgumentNullException("account");

            IssueCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + evt.Account.Tenant, evt.Token);
        }
        
        public void Handle(TwoFactorAuthenticationDisabledEvent<TAccount> evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");
            if (evt.Account == null) throw new ArgumentNullException("account");

            RemoveCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + evt.Account.Tenant);
        }
    }
    
    public abstract class CookieBasedTwoFactorAuthPolicy : CookieBasedTwoFactorAuthPolicy<UserAccount>
    {
    }
}
