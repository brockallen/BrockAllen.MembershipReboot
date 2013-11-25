/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BrockAllen.MembershipReboot
{
    public interface IPasswordResetSecret
    {
        Guid ID { get; set; }
        string Question { get; set; }
        string Answer { get; set; }
    }
}
