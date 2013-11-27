/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class DelegateValidator<TAccount> : IValidator<TAccount>
        where TAccount : UserAccount
    {
        Func<UserAccountService<TAccount>, TAccount, string, ValidationResult> func;
        public DelegateValidator(Func<UserAccountService<TAccount>, TAccount, string, ValidationResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this.func = func;
        }

        public ValidationResult Validate(UserAccountService<TAccount> service, TAccount account, string value)
        {
            return func(service, account, value);
        }
    }
}
