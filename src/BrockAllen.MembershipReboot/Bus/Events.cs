/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot
{
    public interface IEvent { }
    public interface IEventSource
    {
        IEnumerable<IEvent> GetEvents();
        void Clear();
    }

    public interface IEventHandler { }
    public interface IEventHandler<in T> : IEventHandler
        where T : IEvent
    {
        void Handle(T evt);
    }
    public class DelegateEventHandler<T> : IEventHandler<T>
        where T : IEvent
    {
        Action<T> action;
        public DelegateEventHandler(Action<T> action)
        {
            if (action == null) throw new ArgumentNullException("action");
            this.action = action;
        }

        public void Handle(T evt)
        {
            action(evt);
        }
    }

    public interface IEventBus
    {
        void RaiseEvent(IEvent evt);
    }
}
