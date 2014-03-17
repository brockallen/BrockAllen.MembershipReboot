namespace BrockAllen.MembershipReboot.Nh.Extensions
{
    using System;
    using System.Reflection;

    using BrockAllen.MembershipReboot.Nh.Mappings;

    using NHibernate.Cfg;
    using NHibernate.Dialect;
    using NHibernate.Mapping.ByCode;

    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Call this method to add class mappings and any MembershipReboot configuration.
        /// This method must be called after a call to Configure().
        /// </summary>
        /// <param name="configuration"></param>
        public static void ConfigureMembershipReboot(this Configuration configuration)
        {
            // Hack to check if Configure() has been called. Should find a better way.
            string dialectName;
            if (!configuration.Properties.TryGetValue("dialect", out dialectName))
            {
                throw new InvalidOperationException("Could not find the dialect in the configuration. Have you called the Configuration.Configure() method?");
            }

            // Register the generators registry class.
            configuration.LinqToHqlGeneratorsRegistry<MembershipRebootLinqToHqlGeneratorsRegistry>();

            // Add mappings for the classes.
            var mapper = new ModelMapper();
            mapper.AddMappings(Assembly.GetAssembly(typeof(UserAccountMapping)).GetExportedTypes());
            var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();
            configuration.AddMapping(mapping);
        }
    }
}