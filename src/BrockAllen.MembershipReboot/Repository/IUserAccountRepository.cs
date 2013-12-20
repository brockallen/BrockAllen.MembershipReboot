/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
namespace BrockAllen.MembershipReboot
{
    public interface IUserAccountRepository<TAccount>
        where TAccount : UserAccount
    {
        TAccount Create();
        void Add(TAccount item);
        void Remove(TAccount item);
        void Update(TAccount item); 
        TAccount GetByID(Guid id);
        TAccount GetByUsername(string username);
        TAccount GetByUsername(string tenant, string username);
        TAccount GetByEmail(string tenant, string email);
        TAccount GetByMobilePhone(string tenant, string phone);
        TAccount GetByVerificationKey(string key);
        TAccount GetByLinkedAccount(string tenant, string provider, string id);
        TAccount GetByCertificate(string tenant, string thumbprint);
    }

    public interface IUserAccountRepository : IUserAccountRepository<UserAccount> { }
}
