/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface IMessageFormatter
    {
        Message Format(UserAccountEvent accountEvent);
    }
}
