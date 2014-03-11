/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface ICommandBus
    {
        void Execute(ICommand cmd);
    }

    public class CommandBus : List<ICommandHandler>, ICommandBus
    {
        public void Execute(ICommand cmd)
        {
            var action = GetAction(cmd);
            var matchingHandlers = GetHandlers(cmd);
            foreach (var handler in matchingHandlers)
            {
                action(handler, cmd);
            }
        }

        ConcurrentDictionary<Type, IEnumerable<ICommandHandler>> handlerCache = new ConcurrentDictionary<Type, IEnumerable<ICommandHandler>>();
        GenericMethodActionBuilder<ICommandHandler, ICommand> actions = new GenericMethodActionBuilder<ICommandHandler, ICommand>(typeof(ICommandHandler<>), "Handle");

        Action<ICommandHandler, ICommand> GetAction(ICommand evt)
        {
            return actions.GetAction(evt);
        }

        private IEnumerable<ICommandHandler> GetHandlers(ICommand cmd)
        {
            var eventType = cmd.GetType();
            if (!handlerCache.ContainsKey(eventType))
            {
                var eventHandlerType = typeof(ICommandHandler<>).MakeGenericType(eventType);
                var query =
                    from handler in this
                    where eventHandlerType.IsAssignableFrom(handler.GetType())
                    select handler;
                var handlers = query.ToArray().Cast<ICommandHandler>();
                handlerCache[eventType] = handlers;
            }
            return handlerCache[eventType];
        }
    }
}
