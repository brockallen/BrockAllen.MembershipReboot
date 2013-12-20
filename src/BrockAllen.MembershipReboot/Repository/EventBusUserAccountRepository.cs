/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class EventBusUserAccountRepository<TAccount> : IUserAccountRepository<TAccount>
        where TAccount : UserAccount
    {
        IEventSource source;
        internal IUserAccountRepository<TAccount> inner;
        IEventBus validationBus;
        IEventBus eventBus;

        public EventBusUserAccountRepository(IEventSource source, IUserAccountRepository<TAccount> inner, IEventBus validationBus, IEventBus eventBus)
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

        public TAccount GetByID(Guid id)
        {
            return inner.GetByID(id);
        }

        public TAccount GetByUsername(string username)
        {
            return inner.GetByUsername(username);
        }

        public TAccount GetByUsername(string tenant, string username)
        {
            return inner.GetByUsername(tenant, username);
        }

        public TAccount GetByEmail(string tenant, string email)
        {
            return inner.GetByEmail(tenant, email);
        }

        public TAccount GetByMobilePhone(string tenant, string phone)
        {
            return inner.GetByMobilePhone(tenant, phone);
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
    }
}
