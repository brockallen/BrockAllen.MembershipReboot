[assembly: WebActivator.PreApplicationStartMethod(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Stop")]

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    using BrockAllen.MembershipReboot;
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
            kernel.Bind<MembershipRebootConfiguration<CustomUserAccount>>().ToConstant(config);
            kernel.Bind<IUserAccountRepository<CustomUserAccount>>().To<CustomRepository>().InRequestScope();
            kernel.Bind<AuthenticationService<CustomUserAccount>>().To<SamAuthenticationService<CustomUserAccount>>();
        }
    }
}
