/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public abstract class CookieBasedTwoFactorAuthPolicy :
        ITwoFactorAuthenticationPolicy,
        IEventHandler<TwoFactorAuthenticationTokenCreatedEvent>,
        IEventHandler<TwoFactorAuthenticationDisabledEvent>
    {
        public CookieBasedTwoFactorAuthPolicy()
        {
            this.PersistentCookieDurationInDays = MembershipRebootConstants.AuthenticationService.DefaultPersistentCookieDays;
        }

        public int PersistentCookieDurationInDays { get; set; }

        protected abstract string GetCookie(string name);
        protected abstract void IssueCookie(string name, string value);
        protected abstract void RemoveCookie(string name);

        public string GetTwoFactorAuthToken(IUserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            return GetCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + account.Tenant);
        }

        public void Handle(TwoFactorAuthenticationTokenCreatedEvent evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");
            if (evt.Token == null) throw new ArgumentNullException("Token");
            if (evt.Account == null) throw new ArgumentNullException("account");

            IssueCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + evt.Account.Tenant, evt.Token);
        }

        public void Handle(TwoFactorAuthenticationDisabledEvent evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");
            if (evt.Account == null) throw new ArgumentNullException("account");

            RemoveCookie(MembershipRebootConstants.AuthenticationService.CookieBasedTwoFactorAuthPolicyCookieName + evt.Account.Tenant);
        }
    }
}
