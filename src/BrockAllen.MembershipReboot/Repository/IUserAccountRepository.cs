/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository<TAccount> : IRepository<TAccount>
        where TAccount : UserAccount
    {
    }

    public interface ISimpleUserAccountRepository<TAccount> : IDisposable where TAccount : UserAccount {
        TAccount Get(Guid key);
        TAccount Create();
        void Add(TAccount item);
        void Remove(TAccount item);
        void Update(TAccount item);
        TAccount GetByUsername(string tenant, string username, bool usernamesUniqueAcrossTenants);
        TAccount GetByEmail(string tenant, string email);
        TAccount GetByVerificationKey(string key);
        TAccount GetByLinkedAccount(string tenant, string provider, string id);
        TAccount GetByCertificate(string tenant, string thumbprint);
        bool UsernameExistsAcrossTenants(string username);
        bool UsernameExists(string tenant, string username);
        bool EmailExists(string tenant, string email);
        bool EmailExistsOtherThan(TAccount account, string email);
        bool IsMobilePhoneNumberUnique(string tenant, Guid accountId, string mobile);
    }

    public interface IUserAccountRepository : IUserAccountRepository<UserAccount> { }
    
    public interface IUserAccountFactory
    {
        LinkedAccount CreateLinkedAccount();
        LinkedAccountClaim CreateLinkedAccountClaim();
        PasswordResetSecret CreatePasswordResetSecret();
        TwoFactorAuthToken CreateTwoFactorAuthToken();
        UserCertificate CreateUserCertificate();
    }
}
