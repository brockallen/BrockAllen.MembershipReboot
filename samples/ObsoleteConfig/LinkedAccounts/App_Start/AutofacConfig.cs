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
        internal static void Register()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<UserAccountService>();
            builder.RegisterType<ClaimsBasedAuthenticationService>();

            builder
                .RegisterType<DefaultUserAccountRepository>()
                .As<IUserAccountRepository>()
                .InstancePerHttpRequest();
            
            builder.RegisterType<NopMessageDelivery>().As<IMessageDelivery>();
            //builder.RegisterType<SmtpMessageDelivery>().As<IMessageDelivery>();
            
            builder.RegisterType<NopPasswordPolicy>().As<IPasswordPolicy>();
            //builder.Register<IPasswordPolicy>(x=>new BasicPasswordPolicy { MinLength = 4 });

            builder.RegisterType<NotificationService>().As<INotificationService>();

            builder.Register<ApplicationInformation>(
                x => 
                {
                    // build URL
                    var baseUrl = HttpContext.Current.GetApplicationUrl();

                    return new ApplicationInformation
                    {
                        ApplicationName = "Test",
                        LoginUrl = baseUrl + "Home/Login/",
                        VerifyAccountUrl = baseUrl + "Home/Confirm/",
                        CancelNewAccountUrl = baseUrl + "Home/Cancel/",
                        ConfirmPasswordResetUrl = baseUrl + "PasswordReset/Confirm/",
                        ConfirmChangeEmailUrl = baseUrl + "ChangeEmail/Confirm/"
                    };
                });

            builder.RegisterControllers(typeof(AutofacConfig).Assembly);
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}