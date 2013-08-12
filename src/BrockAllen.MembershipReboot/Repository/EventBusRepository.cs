/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class EventBusRepository<T> : IRepository<T>
        where T : class, IEventSource
    {
        IRepository<T> inner;
        IEventBus validationBus;
        IEventBus eventBus;

        public EventBusRepository(IRepository<T> inner, IEventBus validationBus, IEventBus eventBus)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (validationBus == null) throw new ArgumentNullException("validationBus");
            if (eventBus == null) throw new ArgumentNullException("eventBus");

            this.inner = inner;
            this.validationBus = validationBus;
            this.eventBus = eventBus;
        }

        private void RaiseValidation(IEventSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            foreach (var evt in source.Events)
            {
                this.validationBus.RaiseEvent(evt);
            }
        }

        private void RaiseEvents(IEventSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            foreach (var evt in source.Events)
            {
                this.eventBus.RaiseEvent(evt);
            }

            source.Clear();
        }

        public IQueryable<T> GetAll()
        {
            return inner.GetAll();
        }

        public T Get(params object[] keys)
        {
            return inner.Get(keys);
        }

        public T Create()
        {
            return inner.Create();
        }

        public void Add(T item)
        {
            RaiseValidation(item);
            inner.Add(item);
            RaiseEvents(item);
        }

        public void Remove(T item)
        {
            RaiseValidation(item);
            inner.Remove(item);
            RaiseEvents(item);
        }

        public void Update(T item)
        {
            RaiseValidation(item);
            inner.Update(item);
            RaiseEvents(item);
        }

        public void Dispose()
        {
            if (inner.TryDispose()) inner = null;
        }
    }
}
