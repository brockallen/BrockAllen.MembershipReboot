using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BrockAllen.MembershipReboot
{
    public abstract class CookieBasedTwoFactorAuthPolicy :
        ITwoFactorAuthenticationPolicy,
        IEventHandler<SuccessfulTwoFactorAuthCodeLoginEvent>
    {
        const string CookieBasedTwoFactorAuthPolicyCookieName = "cbtfap";

        protected abstract bool HasCookie(string name, string value);
        protected abstract void IssueCookie(string name, string value);

        string GetCookieValue(UserAccount account)
        {
            return CryptoHelper.Hash(account.ID.ToString(), account.HashedPassword);
        }

        public bool RequestRequiresTwoFactorAuth(UserAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");

            if (HasCookie(CookieBasedTwoFactorAuthPolicyCookieName, GetCookieValue(account)))
            {
                return false;
            }

            return true;
        }

        public void Handle(SuccessfulTwoFactorAuthCodeLoginEvent evt)
        {
            if (evt == null) throw new ArgumentNullException("evt");

            IssueCookie(CookieBasedTwoFactorAuthPolicyCookieName, GetCookieValue(evt.Account));
        }
    }
}
