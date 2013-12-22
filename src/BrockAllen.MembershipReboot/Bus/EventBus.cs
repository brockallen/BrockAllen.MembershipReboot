/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BrockAllen.MembershipReboot
{
    class EventBus : List<IEventHandler>, IEventBus
    {
        Dictionary<Type, IEnumerable<IEventHandler>> handlerCache = new Dictionary<Type, IEnumerable<IEventHandler>>();
        GenericMethodActionBuilder<IEventHandler, IEvent> actions = new GenericMethodActionBuilder<IEventHandler, IEvent>(typeof(IEventHandler<>), "Handle");

        public void RaiseEvent(IEvent evt)
        {
            var action = GetAction(evt);
            var matchingHandlers = GetHandlers(evt);
            foreach (var handler in matchingHandlers)
            {
                action(handler, evt);
            }
        }

        Action<IEventHandler, IEvent> GetAction(IEvent evt)
        {
            return actions.GetAction(evt);
        }

        private IEnumerable<IEventHandler> GetHandlers(IEvent evt)
        {
            var eventType = evt.GetType();
            if (!handlerCache.ContainsKey(eventType))
            {
                var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                var query =
                    from handler in this
                    where eventHandlerType.IsAssignableFrom(handler.GetType())
                    select handler;
                var handlers = query.ToArray().Cast<IEventHandler>();
                handlerCache.Add(eventType, handlers);
            }
            return handlerCache[eventType];
        }
    }

    class AggregateEventBus : List<IEventBus>, IEventBus
    {
        public void RaiseEvent(IEvent evt)
        {
            foreach (var eb in this)
            {
                eb.RaiseEvent(evt);
            }
        }
    }
}
