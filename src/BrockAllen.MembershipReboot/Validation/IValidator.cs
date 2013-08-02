/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public interface IValidator
    {
        ValidationResult Validate(UserAccountService service, UserAccount account, string value);
    }
}
