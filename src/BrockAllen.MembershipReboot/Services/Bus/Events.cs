using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface IEvent { }
    public interface IEventSource
    {
        IEnumerable<IEvent> Events { get; }
        void Clear();
    }

    public interface IEventHandler { }
    public interface IEventHandler<T> : IEventHandler
        where T : IEvent
    {
        void Handle(T evt);
    }

    public interface IEventBus
    {
        void RaiseEvent(IEvent evt);
    }
}
