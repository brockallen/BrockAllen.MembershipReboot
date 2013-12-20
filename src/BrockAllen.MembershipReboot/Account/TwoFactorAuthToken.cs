/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class TwoFactorAuthToken
    {
        [StringLength(100)]
        public virtual string Token { get; protected internal set; }

        public virtual DateTime Issued { get; protected internal set; }
    }
}
