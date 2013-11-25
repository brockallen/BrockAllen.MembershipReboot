using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    internal static class IUserAccountInternalExtensions
    {
        //internal static void Init(this IUserAccount account, string tenant, string username, string password, string email)
        //{
        //    Tracing.Information("[UserAccount.Init] called");

        //    if (String.IsNullOrWhiteSpace(tenant))
        //    {
        //        Tracing.Error("[UserAccount.Init] failed -- no tenant");
        //        throw new ArgumentNullException("tenant");
        //    }
        //    if (String.IsNullOrWhiteSpace(username))
        //    {
        //        Tracing.Error("[UserAccount.Init] failed -- no username");
        //        throw new ValidationException(Resources.ValidationMessages.UsernameRequired);
        //    }
        //    if (String.IsNullOrWhiteSpace(password))
        //    {
        //        Tracing.Error("[UserAccount.Init] failed -- no password");
        //        throw new ValidationException(Resources.ValidationMessages.PasswordRequired);
        //    }
        //    if (String.IsNullOrWhiteSpace(email))
        //    {
        //        Tracing.Error("[UserAccount.Init] failed -- no email");
        //        throw new ValidationException(Resources.ValidationMessages.EmailRequired);
        //    }

        //    if (account.ID != Guid.Empty)
        //    {
        //        Tracing.Error("[UserAccount.Init] failed -- ID already assigned");
        //        throw new Exception("Can't call Init if UserAccount is already assigned an ID");
        //    }

        //    account.ID = Guid.NewGuid();
        //    account.Tenant = tenant;
        //    account.Username = username;
        //    account.Email = email;
        //    account.Created = UtcNow;
        //    account.LastUpdated = account.Created;
        //    account.HashedPassword = Configuration.Crypto.HashPassword(password);
        //    account.PasswordChanged = account.Created;
        //    account.IsAccountVerified = false;
        //    account.IsLoginAllowed = false;
        //    account.AccountTwoFactorAuthMode = TwoFactorAuthMode.None;
        //    account.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.None;
        //    var key = this.SetVerificationKey(VerificationKeyPurpose.VerifyAccount);

        //    this.AddEvent(new AccountCreatedEvent { Account = account, VerificationKey = key });
        //}

        //internal string SetVerificationKey(this IUserAccount account, VerificationKeyPurpose purpose, string key = null, string state = null)
        //{
        //    if (key == null) key = UserAccountService.StripUglyBase64(Configuration.Crypto.GenerateSalt());

        //    this.VerificationKey = CryptoHelper.Hash(key);
        //    this.VerificationPurpose = purpose;
        //    this.VerificationKeySent = UtcNow;
        //    this.VerificationStorage = state;

        //    return key;
        //}

    }
}
