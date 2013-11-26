/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public interface IValidator<T>
        where T : UserAccount
    {
        ValidationResult Validate(UserAccountService<T> service, T account, string value);
    }
}
    