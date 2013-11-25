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
        internal TwoFactorAuthToken()
        {
        }

        [Key]
        [Column(Order = 1)]
        public virtual Guid UserAccountID { get; internal set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public virtual string Token { get; internal set; }

        public virtual DateTime Issued { get; internal set; }
    }
}
