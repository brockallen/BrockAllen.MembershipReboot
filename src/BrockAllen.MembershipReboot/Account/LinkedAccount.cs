/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot
{
    public interface ILinkedAccount
    {
        string ProviderName { get; set; }
        string ProviderAccountID { get; set; }
        DateTime LastLogin { get; set; }
        ICollection<IUserClaim> Claims { get; set; }

        IUserClaim CreateClaim();
    }
}
