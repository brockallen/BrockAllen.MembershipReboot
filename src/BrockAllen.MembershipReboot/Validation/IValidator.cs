/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public interface IValidator<TAccount>
        where TAccount : UserAccount
    {
        ValidationResult Validate(UserAccountService<TAccount> service, TAccount account, string value);
    }
}
    