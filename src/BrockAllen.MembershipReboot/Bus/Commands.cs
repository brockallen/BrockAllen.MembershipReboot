/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System;
namespace BrockAllen.MembershipReboot
{
    public interface ICommand
    { }

    public interface ICommandHandler {}
    public interface ICommandHandler<in TCommand> : ICommandHandler
        where TCommand : ICommand
    {
        void Handle(TCommand cmd);
    }

    public class DelegateCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        Action<TCommand> action;
        public DelegateCommandHandler(Action<TCommand> action)
        {
            this.action = action;
        }

        public void Handle(TCommand cmd)
        {
            action(cmd);
        }
    }
}
