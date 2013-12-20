/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class TwoFactorAuthToken
    {
        [StringLength(100)]
        public virtual string Token { get; protected internal set; }

        public virtual DateTime Issued { get; protected internal set; }
    }
}
