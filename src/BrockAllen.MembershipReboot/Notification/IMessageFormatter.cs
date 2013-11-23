/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Collections.Generic;
namespace BrockAllen.MembershipReboot
{
    public interface IMessageFormatter
    {
        Message Format(UserAccountEvent accountEvent, dynamic extra);
    }
}
