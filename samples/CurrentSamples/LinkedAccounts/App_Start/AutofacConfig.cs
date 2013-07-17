using Autofac;
using Autofac.Integration.Mvc;
using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LinkedAccounts
{
    public class AutofacConfig
    {
        public static MembershipRebootConfiguration CreateMembershipRebootConfiguration()
        {
            var settings = SecuritySettings.Instance;
            settings.MultiTenant = false;

            var config = new MembershipRebootConfiguration(settings, new DelegateFactory(() => new EFUserAccountRepository(settings.ConnectionStringName)));
            return config;
        }

        internal static void Register()
        {
            var config = CreateMembershipRebootConfiguration();

            var builder = new ContainerBuilder();

            builder.RegisterType<UserAccountService>();
            builder.RegisterType<ClaimsBasedAuthenticationService>();

            builder
                .Register<EFUserAccountRepository>(x=>new EFUserAccountRepository(config.SecuritySettings.ConnectionStringName))
                .As<IUserAccountRepository>()
                .InstancePerHttpRequest();

            builder.RegisterControllers(typeof(AutofacConfig).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}