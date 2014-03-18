namespace BrockAllen.MembershipReboot.Nh.Extensions
{
    using System;

    using NHibernate.Linq;
    using NHibernate.Linq.Functions;

    public class MembershipRebootLinqToHqlGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public MembershipRebootLinqToHqlGeneratorsRegistry() : base()
        {
            // Register the string.Equals(string, StringComparison) generator.
            this.RegisterGenerator(
                ReflectionHelper.GetMethodDefinition<string>(x => x.Equals(null, StringComparison.CurrentCulture)),
                new StringComparisonEqualsGenerator());
        }
    }
}