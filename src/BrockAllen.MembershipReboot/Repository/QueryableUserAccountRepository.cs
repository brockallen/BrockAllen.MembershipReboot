/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public abstract class QueryableUserAccountRepository<TAccount> : IUserAccountRepository<TAccount>
        where TAccount : UserAccount
    {
        protected abstract IQueryable<TAccount> Queryable { get; }
        public abstract TAccount Create();
        public abstract void Add(TAccount item);
        public abstract void Remove(TAccount item);
        public abstract void Update(TAccount item);

        public TAccount GetByID(Guid id)
        {
            return Queryable.SingleOrDefault(x => x.ID == id);
        }

        public TAccount GetByUsername(string username)
        {
            return Queryable.SingleOrDefault(x => x.Username == username);
        }

        public TAccount GetByUsername(string tenant, string username)
        {
            return Queryable.SingleOrDefault(x => x.Tenant == tenant && x.Username == username);
        }

        public TAccount GetByEmail(string tenant, string email)
        {
            return Queryable.SingleOrDefault(x => x.Tenant == tenant && x.Email == email);
        }

        public TAccount GetByMobilePhone(string tenant, string phone)
        {
            return Queryable.SingleOrDefault(x => x.Tenant == tenant && x.MobilePhoneNumber == phone);
        }

        public TAccount GetByVerificationKey(string key)
        {
            return Queryable.SingleOrDefault(x => x.VerificationKey == key);
        }

        public abstract TAccount GetByLinkedAccount(string tenant, string provider, string id);
        public abstract TAccount GetByCertificate(string tenant, string thumbprint);
    }
}
