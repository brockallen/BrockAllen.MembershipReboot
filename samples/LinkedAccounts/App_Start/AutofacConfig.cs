using Autofac;
using Autofac.Integration.Mvc;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using System.Web.Mvc;

namespace LinkedAccounts
{
    public class AutofacConfig
    {
        public static MembershipRebootConfiguration CreateMembershipRebootConfiguration()
        {
            var settings = SecuritySettings.Instance;
            settings.MultiTenant = false;

            var config = new MembershipRebootConfiguration(settings);
            return config;
        }

        internal static void Register()
        {
            var config = CreateMembershipRebootConfiguration();
            var appinfo = new AspNetApplicationInformation("Test", "Test Email Signature",
                            "UserAccount/Login",
                            "UserAccount/Register/Confirm/",
                            "UserAccount/Register/Cancel/",
                            "UserAccount/PasswordReset/Confirm/",
                            "UserAccount/ChangeEmail/Confirm/");
            var emailFormatter = new EmailMessageFormatter(appinfo); 
            // uncomment if you want email notifications -- also update smtp settings in web.config
            //config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

            var builder = new ContainerBuilder();

            builder.RegisterType<UserAccountService>();
            builder.RegisterInstance<MembershipRebootConfiguration>(config);
            builder.RegisterType<SamAuthenticationService>().As<AuthenticationService>();
            builder
                .Register<DefaultUserAccountRepository>(x=>new DefaultUserAccountRepository())
                .As<IUserAccountRepository>()
                .InstancePerHttpRequest();

            builder.RegisterControllers(typeof(AutofacConfig).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}