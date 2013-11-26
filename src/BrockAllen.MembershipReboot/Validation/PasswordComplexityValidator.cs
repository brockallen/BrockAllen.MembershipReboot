using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class PasswordComplexityValidator<T> : IValidator<T>
        where T : UserAccount
    {
        public int MinimumLength { get; set; }
        public int MinimumNumberOfComplexityRules { get; set; }

        public PasswordComplexityValidator()
            : this(MembershipRebootConstants.PasswordComplexity.MinimumLength, MembershipRebootConstants.PasswordComplexity.NumberOfComplexityRules)
        {
        }

        public PasswordComplexityValidator(int minimumLength, int minimumNumberOfComplexityRules)
        {
            this.MinimumLength = minimumLength;
            this.MinimumNumberOfComplexityRules = minimumNumberOfComplexityRules;
            if (MinimumNumberOfComplexityRules > 4) MinimumNumberOfComplexityRules = 4;
        }

        public ValidationResult Validate(UserAccountService<T> service, T account, string value)
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return new ValidationResult(Resources.ValidationMessages.PasswordRequired);
            }
            
            if (value.Length < this.MinimumLength)
            {
                return new ValidationResult(String.Format(Resources.ValidationMessages.PasswordLength, this.MinimumLength));
            }

            var upper = value.Any(x => Char.IsUpper(x));
            var lower = value.Any(x => Char.IsLower(x));
            var digit = value.Any(x => Char.IsDigit(x));
            var other = value.Any(x => !Char.IsUpper(x) && !Char.IsLower(x) && !Char.IsDigit(x));

            var vals = new bool[] { upper, lower, digit, other };
            var matches = vals.Where(x => x).Count();
            if (matches < this.MinimumNumberOfComplexityRules)
            {
                return new ValidationResult(String.Format(Resources.ValidationMessages.PasswordComplexityRules, this.MinimumNumberOfComplexityRules));
            }

            return null;
        }
    }
}
