/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public interface IValidator
    {
        ValidationResult Validate(UserAccountService service, IUserAccount account, string value);
    }
}
    