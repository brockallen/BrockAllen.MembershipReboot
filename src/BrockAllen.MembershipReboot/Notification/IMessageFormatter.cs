/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Collections.Generic;
namespace BrockAllen.MembershipReboot
{
    public interface IMessageFormatter<T>
        where T : UserAccount
    {
        Message Format(UserAccountEvent<T> accountEvent, dynamic extra);
    }
}
