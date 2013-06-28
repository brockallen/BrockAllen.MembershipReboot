/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public class NopMessageDelivery : IMessageDelivery
    {
        public void Send(Message msg)
        {
        }
    }
}
