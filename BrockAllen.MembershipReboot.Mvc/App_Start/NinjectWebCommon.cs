[assembly: WebActivator.PreApplicationStartMethod(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(BrockAllen.MembershipReboot.Mvc.App_Start.NinjectWebCommon), "Stop")]

namespace BrockAllen.MembershipReboot.Mvc.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;
    using BrockAllen.MembershipReboot;

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
            kernel.Bind<IUserAccountRepository>().ToMethod(x => new EFUserAccountRepository(Constants.ConnectionName));
            
            //kernel.Bind<IMessageDelivery>().To<NopMessageDelivery>();
            kernel.Bind<IMessageDelivery>().To<SmtpMessageDelivery>();
            
            kernel.Bind<ApplicationInformation>()
                .ToMethod(x=>
                    {
                        // build URL
                        var ctx = HttpContext.Current;
                        var baseUrl =
                            ctx.Request.Url.Scheme + 
                            "://" + 
                            ctx.Request.Url.Host + (ctx.Request.Url.Port == 80 ? "" : ":" + ctx.Request.Url.Port) + 
                            ctx.Request.ApplicationPath;
                        if (!baseUrl.EndsWith("/")) baseUrl += "/";
                        
                        // area name
                        baseUrl += "UserAccount/";
                        
                        return new ApplicationInformation { 
                            ApplicationName="Test",
                            LoginUrl = baseUrl + "Login",
                            VerifyAccountUrl = baseUrl + "Register/Confirm/",
                            CancelNewAccountUrl = baseUrl + "Register/Cancel/",
                            ConfirmPasswordResetUrl = baseUrl + "PasswordReset/Confirm/",
                            ConfirmChangeEmailUrl = baseUrl + "ChangeEmail/Confirm/"
                        };
                    });

            //kernel.Bind<IPasswordPolicy>().To<NopPasswordPolicy>();
            kernel.Bind<IPasswordPolicy>().ToMethod(x => new BasicPasswordPolicy { MinLength = 4 });
        }        
    }
}
