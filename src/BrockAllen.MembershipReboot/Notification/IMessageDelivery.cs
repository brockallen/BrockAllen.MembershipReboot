/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public interface IMessageDelivery
    {
        void Send(Message msg);
    }
}
