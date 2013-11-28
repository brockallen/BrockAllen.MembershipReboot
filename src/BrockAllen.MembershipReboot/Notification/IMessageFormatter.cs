/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


using System.Collections.Generic;
namespace BrockAllen.MembershipReboot
{
    public interface IMessageFormatter<TAccount>
        where TAccount : UserAccount
    {
        Message Format(UserAccountEvent<TAccount> accountEvent, IDictionary<string, string> values);
    }
}
