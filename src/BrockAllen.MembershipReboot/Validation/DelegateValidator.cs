/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class DelegateValidator<T> : IValidator<T>
        where T : UserAccount
    {
        Func<UserAccountService<T>, T, string, ValidationResult> func;
        public DelegateValidator(Func<UserAccountService<T>, T, string, ValidationResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this.func = func;
        }

        public ValidationResult Validate(UserAccountService<T> service, T account, string value)
        {
            return func(service, account, value);
        }
    }
}
