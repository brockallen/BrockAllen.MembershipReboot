/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class GroupChild
    {
        public int ID { get; set; }
        public Guid Parent { get; set; }
        public Guid Group { get; set; }
    }
}
