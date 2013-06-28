/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class EventBus : List<IEventHandler>, IEventBus
    {
        Dictionary<Type, IEnumerable<IEventHandler>> handlerCache = new Dictionary<Type, IEnumerable<IEventHandler>>();
        Dictionary<Type, Action<IEventHandler, IEvent>> actionCache = new Dictionary<Type, Action<IEventHandler, IEvent>>();

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
            var eventType = evt.GetType();

            if (!actionCache.ContainsKey(eventType))
            {
                actionCache.Add(eventType, BuildHandlerInvocation(eventType));
            }

            return actionCache[eventType];
        }

        private Action<IEventHandler, IEvent> BuildHandlerInvocation(Type eventType)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

            var ehParam = Expression.Parameter(typeof(IEventHandler));
            var evtParam = Expression.Parameter(typeof(IEvent));
            var invocationExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Convert(ehParam, handlerType),
                            handlerType.GetMethod("Handle"),
                            Expression.Convert(evtParam, eventType))),
                    ehParam, evtParam);

            return (Action<IEventHandler, IEvent>)invocationExpression.Compile();
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
}
