using Autofac;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Web;

[assembly: Microsoft.Owin.OwinStartup(typeof(OwinHostSample.Startup))]

namespace OwinHostSample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMembershipReboot(app);
            app.UseNancy();
        }

        private static void ConfigureMembershipReboot(IAppBuilder app)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            var cookieOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationType
            };

            var appInfo = new OwinApplicationInformation(
                app,
                "Test",
                "Test Email Signature",
                "/Login",
                "/Register/Confirm/",
                "/Register/Cancel/",
                "/PasswordReset/Confirm/");

            var config = new MembershipRebootConfiguration();
            var emailFormatter = new EmailMessageFormatter(appInfo);
            // uncomment if you want email notifications -- also update smtp settings in web.config
            config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));

            var builder = new ContainerBuilder();
            
            builder.RegisterInstance(config).As<MembershipRebootConfiguration>();
            
            builder.RegisterType<DefaultUserAccountRepository>()
                .As<IUserAccountRepository>()
                .As<IUserAccountQuery>()
                .InstancePerLifetimeScope();
            
            builder.RegisterType<UserAccountService>().OnActivating(e =>
            {
                var owin = e.Context.Resolve<IOwinContext>();
                var debugging = false;
                #if DEBUG
                    debugging = true;
                #endif
                e.Instance.ConfigureTwoFactorAuthenticationCookies(owin.Environment, debugging);
            })
            .AsSelf()
            .InstancePerLifetimeScope();
            
            builder.Register(ctx =>
            {
                var owin = ctx.Resolve<IOwinContext>();
                return new OwinAuthenticationService(cookieOptions.AuthenticationType, ctx.Resolve<UserAccountService>(), owin.Environment);
            })
            .As<AuthenticationService>()
            .InstancePerLifetimeScope();
            
            var container = builder.Build();
            app.Use(async (ctx, next) =>
            {
                using (var scope = container.BeginLifetimeScope(b =>
                {
                    b.RegisterInstance(ctx).As<IOwinContext>();
                }))
                {
                    ctx.Environment.SetUserAccountService(()=>scope.Resolve<UserAccountService>());
                    ctx.Environment.SetAuthenticationService(() =>scope.Resolve<AuthenticationService>());
                    await next();
                }
            });
            app.UseMembershipReboot(cookieOptions);
        }
    }
}