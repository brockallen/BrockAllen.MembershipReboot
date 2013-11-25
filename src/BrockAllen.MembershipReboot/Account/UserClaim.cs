/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public interface IUserClaim
    {
        string Type { get; set; }
        string Value { get; set; }
    }
}
