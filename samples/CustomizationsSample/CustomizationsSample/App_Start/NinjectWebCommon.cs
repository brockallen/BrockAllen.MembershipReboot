[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(CustomizationsSample.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(CustomizationsSample.App_Start.NinjectWebCommon), "Stop")]

namespace CustomizationsSample.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using BrockAllen.MembershipReboot.Mvc.App_Start;
    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Mvc;
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
            var config = MembershipRebootConfig.Create();
            kernel.Bind<MembershipRebootConfiguration<CustomUserAccount>>().ToConstant(config);
            kernel.Bind<IUserAccountRepository<CustomUserAccount>>().To<CustomRepository>().InRequestScope();
            kernel.Bind<CustomDatabase>().ToSelf().InRequestScope();
            kernel.Bind<IUserAccountQuery>().To<CustomRepository>().InRequestScope();
            kernel.Bind<AuthenticationService<CustomUserAccount>>().To<SamAuthenticationService<CustomUserAccount>>();
        }        
    }
}
