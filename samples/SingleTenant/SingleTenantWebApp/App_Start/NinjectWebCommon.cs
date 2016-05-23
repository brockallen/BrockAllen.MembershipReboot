[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Stop")]

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using System.Data.Entity;
    using BrockAllen.MembershipReboot.Ef;
    using BrockAllen.MembershipReboot.WebHost;

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
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());

            var config = MembershipRebootConfig.Create();
            kernel.Bind<MembershipRebootConfiguration>().ToConstant(config);
            kernel.Bind<DefaultMembershipRebootDatabase>().ToSelf();
            kernel.Bind<UserAccountService>().ToSelf();
            kernel.Bind<AuthenticationService>().To<SamAuthenticationService>();
            kernel.Bind<IUserAccountQuery>().To<DefaultUserAccountRepository>().InRequestScope();
            kernel.Bind<IUserAccountRepository>().To<DefaultUserAccountRepository>().InRequestScope();
        }        
    }
}
