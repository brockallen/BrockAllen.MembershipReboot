using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
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
