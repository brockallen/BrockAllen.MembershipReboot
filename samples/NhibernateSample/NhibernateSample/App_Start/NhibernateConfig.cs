namespace NhibernateSample
{
    using BrockAllen.MembershipReboot.Nh.Extensions;

    using NHibernate;
    using NHibernate.Cfg;

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
            config.ConfigureMembershipReboot();
            return config;
        }
    }
}