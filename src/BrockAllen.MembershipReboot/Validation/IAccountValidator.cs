using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface IValidator
    {
        ValidationResult Validate(UserAccountService service, UserAccount account, string value);
    }

    public class DelegateValidator : IValidator
    {
        Func<UserAccountService, UserAccount, string, ValidationResult> func;
        public DelegateValidator(Func<UserAccountService, UserAccount, string, ValidationResult> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            this.func = func;
        }

        public ValidationResult Validate(UserAccountService service, UserAccount account, string value)
        {
            return func(service, account, value);
        }
    }

    public class AggregateValidator : List<IValidator>, IValidator
    {
        public ValidationResult Validate(UserAccountService service, UserAccount account, string value)
        {
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
