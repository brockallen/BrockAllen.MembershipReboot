using Autofac;
using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ServerApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureMembershipReboot(app);

            var oauthServerConfig = new Microsoft.Owin.Security.OAuth.OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                Provider = new MyProvider(),
                TokenEndpointPath = new PathString("/token")
            };
            app.UseOAuthAuthorizationServer(oauthServerConfig);

            var oauthConfig = new Microsoft.Owin.Security.OAuth.OAuthBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                AuthenticationType = "Bearer"
            };
            app.UseOAuthBearerAuthentication(oauthConfig);

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);
        }

        private static void ConfigureMembershipReboot(IAppBuilder app)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());

            var builder = new ContainerBuilder();

            var config = new MembershipRebootConfiguration();
            // just for giggles, we'll use the multi-tenancy to keep
            // client authentication separate from user authentication
            config.MultiTenant = true;
            config.RequireAccountVerification = false;
            
            builder.RegisterInstance(config);

            builder.RegisterType<DefaultMembershipRebootDatabase>()
                .InstancePerLifetimeScope();
            
            builder.RegisterType<DefaultUserAccountRepository>()
                .As<IUserAccountRepository>()
                .As<IUserAccountQuery>()
                .InstancePerLifetimeScope();

            builder.RegisterType<UserAccountService>()
                .InstancePerLifetimeScope();

            var container = builder.Build();
            app.Use(async (ctx, next) =>
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    ctx.Environment.SetUserAccountService(() => scope.Resolve<UserAccountService>());
                    await next();
                }
            });

            PopulateTestData(container);
        }

        private static void PopulateTestData(IContainer container)
        {
            using(var scope = container.BeginLifetimeScope())
            {
                var svc = scope.Resolve<UserAccountService>();
                if (svc.GetByUsername("clients", "client") == null)
                {
                    var client = svc.CreateAccount("clients", "client", "secret", (string)null);
                    svc.AddClaim(client.ID, "scope", "foo");
                    svc.AddClaim(client.ID, "scope", "bar");
                }
                if (svc.GetByUsername("users", "alice") == null)
                {
                    var alice = svc.CreateAccount("users", "alice", "pass", "alice@alice.com");
                    svc.AddClaim(alice.ID, "role", "people");
                }
            }
        }
    }

    public class MyProvider : OAuthAuthorizationServerProvider
    {
        public override System.Threading.Tasks.Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string cid, csecret;
            if (context.TryGetBasicCredentials(out cid, out csecret))
            {
                var svc = context.OwinContext.Environment.GetUserAccountService<UserAccount>();
                if (svc.Authenticate("clients", cid, csecret))
                {
                    context.Validated();
                }
            }
            return Task.FromResult<object>(null);
        }

        public override Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {
            if (context.TokenRequest.IsResourceOwnerPasswordCredentialsGrantType)
            {
                var svc = context.OwinContext.Environment.GetUserAccountService<UserAccount>();
                var client = svc.GetByUsername("clients", context.ClientContext.ClientId);
                var scopes = context.TokenRequest.ResourceOwnerPasswordCredentialsGrant.Scope;
                if (scopes.All(scope=>client.HasClaim("scope", scope)))
                {
                    context.Validated();
                }
            }
            return Task.FromResult<object>(null);
        }

        public override Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var svc = context.OwinContext.Environment.GetUserAccountService<UserAccount>();
            UserAccount user;
            if (svc.Authenticate("users", context.UserName, context.Password, out user))
            {
                var claims = user.GetAllClaims();

                var id = new System.Security.Claims.ClaimsIdentity(claims, "MembershipReboot");
                context.Validated(id);
            }
            
            return base.GrantResourceOwnerCredentials(context);
        }
    }
}