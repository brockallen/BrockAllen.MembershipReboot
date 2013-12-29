/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
namespace BrockAllen.MembershipReboot
{
    public class GroupQueryResult
    {
        public Guid ID { get; set; }
        public string Tenant { get; set; }
        public string Name { get; set; }
    }
}
