namespace NhibernateSample
{
    using System.Reflection;

    using BrockAllen.MembershipReboot.Nh.Mappings;

    using NHibernate;
    using NHibernate.Cfg;
    using NHibernate.Cfg.MappingSchema;
    using NHibernate.Mapping.ByCode;

    public class NhibernateConfig
    {
        public static ISessionFactory GetSessionFactory()
        {
            var config = GetConfiguration();
            config.BuildMappings();
            return config.BuildSessionFactory();
        }

        private static Configuration GetConfiguration()
        {
            var config = new Configuration();
            config.Configure();
            config.AddMapping(GetMappings());
            return config;
        }

        private static HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();

            mapper.AddMappings(Assembly.GetAssembly(typeof(UserAccountMapping)).GetExportedTypes());
            var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

            return mapping;
        }
    }
}