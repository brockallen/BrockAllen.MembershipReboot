using Autofac;
using Autofac.Integration.Mvc;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Owin;
using BrockAllen.MembershipReboot.Relational;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace SingleTenantOwinSystemWeb
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "External", 
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive
            });
            app.UseFacebookAuthentication(new FacebookAuthenticationOptions
            {
                AppId = "260581164087472", AppSecret = "7389d78e6e629954a710351830d080f3",
                SignInAsAuthenticationType = "External"
            });
            app.UseGoogleAuthentication(new GoogleAuthenticationOptions
            {
                SignInAsAuthenticationType="External"
            });

            ConfigureMembershipReboot(app);
        }

        private static void ConfigureMembershipReboot(IAppBuilder app)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            //System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<DefaultMembershipRebootDatabase>());
            var cookieOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = MembershipRebootOwinConstants.AuthenticationType
            };
            BuildAutofacContainer(app, cookieOptions.AuthenticationType);
            app.UseMembershipReboot(cookieOptions);
        }

        private static void BuildAutofacContainer(IAppBuilder app, string authType)
        {
            var builder = new ContainerBuilder();

            var config = CreateMembershipRebootConfiguration(app);

            builder.RegisterInstance(config).As<MembershipRebootConfiguration>();

            builder.RegisterType<DefaultMembershipRebootDatabase>()
                .InstancePerLifetimeScope();

            builder.RegisterType<DefaultUserAccountRepository>()
                .As<IUserAccountRepository>()
                .As<IUserAccountRepository<RelationalUserAccount>>()
                .As<IUserAccountQuery>()
                .As<IUserAccountQuery<BrockAllen.MembershipReboot.Relational.RelationalUserAccount>>()
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

            builder.RegisterType<UserAccountService<RelationalUserAccount>>().OnActivating(e =>
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
                return new OwinAuthenticationService(authType, ctx.Resolve<UserAccountService>(), owin.Environment);
            })
            .As<AuthenticationService>()
            .InstancePerLifetimeScope();

            builder.Register(ctx=>HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            builder.RegisterControllers(typeof(Startup).Assembly);

            var container = builder.Build();
            System.Web.Mvc.DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static MembershipRebootConfiguration CreateMembershipRebootConfiguration(IAppBuilder app)
        {
            var config = new MembershipRebootConfiguration();
            config.RequireAccountVerification = false;
            config.AddEventHandler(new DebuggerEventHandler());

            var appInfo = new OwinApplicationInformation(
                app,
                "Test",
                "Test Email Signature",
                "/UserAccount/Login",
                "/UserAccount/ChangeEmail/Confirm/",
                "/UserAccount/Register/Cancel/",
                "/UserAccount/PasswordReset/Confirm/");

            var emailFormatter = new EmailMessageFormatter(appInfo);
            // uncomment if you want email notifications -- also update smtp settings in web.config
            config.AddEventHandler(new EmailAccountEventsHandler(emailFormatter));
            // uncomment to enable SMS notifications -- also update TwilloSmsEventHandler class below
            //config.AddEventHandler(new TwilloSmsEventHandler(appinfo));

            // uncomment to ensure proper password complexity
            //config.ConfigurePasswordComplexity();
            
            return config;
        }
    }

    public class TwilloSmsEventHandler : SmsEventHandler
    {
        const string sid = "";
        const string token = "";
        const string fromPhone = "";

        public TwilloSmsEventHandler(ApplicationInformation appInfo)
            : base(new SmsMessageFormatter(appInfo))
        {
        }

        string Url
        {
            get
            {
                return String.Format("https://api.twilio.com/2010-04-01/Accounts/{0}/SMS/Messages", sid);
            }
        }

        string BasicAuthToken
        {
            get
            {
                var val = sid + ":" + token;
                var bytes = System.Text.Encoding.UTF8.GetBytes(val);
                val = Convert.ToBase64String(bytes);
                return val;
            }
        }

        HttpContent GetBody(Message msg)
        {
            var values = new KeyValuePair<string, string>[]
            { 
                new KeyValuePair<string, string>("From", fromPhone),
                new KeyValuePair<string, string>("To", msg.To),
                new KeyValuePair<string, string>("Body", msg.Body),
            };

            return new FormUrlEncodedContent(values);
        }

        protected override void SendSms(Message message)
        {
            if (!String.IsNullOrWhiteSpace(sid))
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", BasicAuthToken);
                var result = client.PostAsync(Url, GetBody(message)).Result;
                result.EnsureSuccessStatusCode();
            }
        }
    }
}