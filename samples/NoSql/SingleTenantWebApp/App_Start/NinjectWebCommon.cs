[assembly: WebActivator.PreApplicationStartMethod(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Stop")]

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Hierarchical;
    using BrockAllen.MembershipReboot.WebHost;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using System;
    using System.Web;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            var config = MembershipRebootConfig.Create();
            var policy = new AspNetCookieBasedTwoFactorAuthPolicy(debugging: true);
            kernel.Bind<UserAccountService<HierarchicalUserAccount>>().ToMethod(ctx =>
            {
                var svc = new UserAccountService<HierarchicalUserAccount>(config, ctx.Kernel.Get<IUserAccountRepository<HierarchicalUserAccount>>());
                svc.TwoFactorAuthenticationPolicy = policy;
                return svc;
            });
<<<<<<< HEAD:samples/SingleTenantAndNoSql/SingleTenantWebApp/App_Start/NinjectWebCommon.cs
            kernel.Bind<AuthenticationService>().To<SamAuthenticationService>();
            RegisterEntityFrameworkSqlAzure(kernel);
            //RegisterEntityFramework(kernel);
            //RegisterMongoDb(kernel);
=======
            kernel.Bind<AuthenticationService<HierarchicalUserAccount>>().To<SamAuthenticationService<HierarchicalUserAccount>>();

            RegisterMongoDb(kernel);
>>>>>>> upstream/master:samples/NoSql/SingleTenantWebApp/App_Start/NinjectWebCommon.cs
            //RegisterRavenDb(kernel);
            
        }

<<<<<<< HEAD:samples/SingleTenantAndNoSql/SingleTenantWebApp/App_Start/NinjectWebCommon.cs
        private static void RegisterEntityFramework(IKernel kernel)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            kernel.Bind<IUserAccountRepository>().ToMethod(ctx => new DefaultUserAccountRepository()).InRequestScope();
        }

        private static void RegisterEntityFrameworkSqlAzure(IKernel kernel)
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<SqlAzureMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.ConfigurationAzure>());
            kernel.Bind<IUserAccountRepository>().ToMethod(ctx => new SqlAzureDefaultUserAccountRepository()).InRequestScope();
        }

=======
>>>>>>> upstream/master:samples/NoSql/SingleTenantWebApp/App_Start/NinjectWebCommon.cs
        // To use MongoDB:
        // - Add a reference to the BrockAllen.MembershipReboot.MongoDb project.
        // - Uncomment this method.
        // - Call this method instead of RegisterEntityFramework in the RegisterServices method above.
        private static void RegisterMongoDb(IKernel kernel)
        {
            kernel.Bind<MongoDb.MongoDatabase>().ToSelf().WithConstructorArgument("connectionStringName", "MongoDb");
            kernel.Bind<IUserAccountRepository<HierarchicalUserAccount>>().To<MongoDb.MongoUserAccountRepository>();
        }
    
        // To use RavenDB::
        // - Add a reference to the BrockAllen.MembershipReboot.RavenDb project.
        // - Uncomment this method.
        // - Call this method instead of RegisterEntityFramework in the RegisterServices method above.
        //private static void RegisterRavenDb(IKernel kernel)
        //{
        //    kernel.Bind<IUserAccountRepository>().ToMethod(ctx => new BrockAllen.MembershipReboot.RavenDb.RavenUserAccountRepository("RavenDb"));
        //}
    }
}
