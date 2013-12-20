﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace BrockAllen.MembershipReboot.Relational
{
    public class RelationalUserClaim : UserClaim
    {
        public Guid UserAccountID { get; set; }
    }
    public class RelationalUserClaimInt : UserClaim
    {
        public int UserAccountID { get; set; }
    }
}
