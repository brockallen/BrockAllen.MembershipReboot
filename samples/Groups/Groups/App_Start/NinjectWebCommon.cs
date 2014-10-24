[assembly: WebActivator.PreApplicationStartMethod(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Stop")]

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Ef;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using RolesAdmin.Models;
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
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<DefaultMembershipRebootDatabase, BrockAllen.MembershipReboot.Ef.Migrations.Configuration>());
            //System.Data.Entity.Database.SetInitializer(new System.Data.Entity.DropCreateDatabaseIfModelChanges<DefaultMembershipRebootDatabase>());
            kernel.Bind<DefaultMembershipRebootDatabase>().ToSelf().InRequestScope();
            kernel.Bind<IGroupRepository>().To<DefaultGroupRepository>().InRequestScope();
            kernel.Bind<IGroupQuery>().To<DefaultGroupRepository>().InRequestScope();
            kernel.Bind<GroupService>().ToMethod(ctx => new GroupService("default", ctx.Kernel.Get<IGroupRepository>()));
        }
    }
}
