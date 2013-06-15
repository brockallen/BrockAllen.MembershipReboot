using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EventBusRepository<T> : IRepository<T>
        where T : class, IEventSource
    {
        IRepository<T> inner;
        IEventBus eventBus;
        public EventBusRepository(IRepository<T> inner, IEventBus eventBus)
        {
            if (inner == null) throw new ArgumentNullException("inner");
            if (eventBus == null) throw new ArgumentNullException("eventBus");

            this.inner = inner;
            this.eventBus = eventBus;
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
            inner.Add(item);
            RaiseEvents(item);
        }

        public void Remove(T item)
        {
            inner.Remove(item);
            RaiseEvents(item);
        }

        public void Update(T item)
        {
            inner.Update(item);
            RaiseEvents(item);
        }

        public void Dispose()
        {
            if (inner.TryDispose()) inner = null;
            if (eventBus is IDisposable)
            {
                ((IDisposable)eventBus).Dispose();
                eventBus = null;
            }
        }
    }
}
