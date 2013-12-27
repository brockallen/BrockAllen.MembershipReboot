/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
