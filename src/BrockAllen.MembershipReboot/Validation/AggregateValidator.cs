/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    public class AggregateValidator<TAccount> : List<IValidator<TAccount>>, IValidator<TAccount>
        where TAccount : UserAccount
    {
        public ValidationResult Validate(UserAccountService<TAccount> service, TAccount account, string value)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (account == null) throw new ArgumentNullException("account");
            
            var list = new List<ValidationResult>();
            foreach (var item in this)
            {
                var result = item.Validate(service, account, value);
                if (result != null && result != ValidationResult.Success)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
