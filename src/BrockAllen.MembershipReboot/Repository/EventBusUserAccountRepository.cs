/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class EventBusUserAccountRepository<TAccount> : ISimpleUserAccountRepository<TAccount>
        where TAccount : UserAccount
    {
        IEventSource source;
        ISimpleUserAccountRepository<TAccount> inner;
        IEventBus validationBus;
        IEventBus eventBus;

        public EventBusUserAccountRepository(IEventSource source, ISimpleUserAccountRepository<TAccount> inner, IEventBus validationBus, IEventBus eventBus)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (inner == null) throw new ArgumentNullException("inner");
            if (validationBus == null) throw new ArgumentNullException("validationBus");
            if (eventBus == null) throw new ArgumentNullException("eventBus");

            this.source = source;
            this.inner = inner;
            this.validationBus = validationBus;
            this.eventBus = eventBus;
        }

        private void RaiseValidation()
        {
            foreach (var evt in source.GetEvents())
            {
                this.validationBus.RaiseEvent(evt);
            }
        }

        private void RaiseEvents()
        {
            foreach (var evt in source.GetEvents())
            {
                this.eventBus.RaiseEvent(evt);
            }

            source.Clear();
        }

        //public IQueryable<TAccount> GetAll()
        //{
        //    return inner.GetAll();
        //}

        public TAccount Get(Guid key)
        {
            return inner.Get(key);
        }

        public TAccount Create()
        {
            return inner.Create();
        }

        public void Add(TAccount item)
        {
            RaiseValidation();
            inner.Add(item);
            RaiseEvents();
        }

        public void Remove(TAccount item)
        {
            RaiseValidation();
            inner.Remove(item);
            RaiseEvents();
        }

        public void Update(TAccount item)
        {
            RaiseValidation();
            inner.Update(item);
            RaiseEvents();
        }

        public TAccount GetByUsername(string tenant, string username, bool usernamesUniqueAcrossTenants)
        {
            return inner.GetByUsername(tenant, username, usernamesUniqueAcrossTenants);
        }

        public TAccount GetByEmail(string tenant, string email)
        {
            return inner.GetByEmail(tenant, email);
        }

        public TAccount GetByVerificationKey(string key)
        {
            return inner.GetByVerificationKey(key);
        }

        public TAccount GetByLinkedAccount(string tenant, string provider, string id)
        {
            return inner.GetByLinkedAccount(tenant, provider, id);
        }

        public TAccount GetByCertificate(string tenant, string thumbprint)
        {
            return inner.GetByCertificate(tenant, thumbprint);
        }

        public bool UsernameExistsAcrossTenants(string username)
        {
            return inner.UsernameExistsAcrossTenants(username);
        }

        public bool UsernameExists(string tenant, string username)
        {
            return inner.UsernameExists(tenant, username);
        }

        public bool EmailExists(string tenant, string email)
        {
            return inner.EmailExists(tenant, email);
        }

        public bool EmailExistsOtherThan(TAccount account, string email)
        {
            return inner.EmailExistsOtherThan(account, email);
        }

        public bool IsMobilePhoneNumberUnique(string tenant, Guid accountId, string mobile)
        {
            return inner.IsMobilePhoneNumberUnique(tenant, accountId, mobile);
        }

        public void Dispose()
        {
            if (inner.TryDispose()) inner = null;
        }
    }
}
