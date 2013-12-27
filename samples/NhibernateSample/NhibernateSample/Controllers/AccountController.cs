namespace NhibernateSample.Controllers
{
    using System.Web.Mvc;

    using BrockAllen.MembershipReboot;
    using BrockAllen.MembershipReboot.Nh;
    using BrockAllen.MembershipReboot.WebHost;

    using NHibernate;

    using NhibernateSample.ViewModels;

    public class AccountController : Controller
    {
        private readonly AuthenticationService<NhUserAccount> authenticationService;

        private readonly ISession session;

        private readonly UserAccountService<NhUserAccount> userAccountService;

        public AccountController(
            AuthenticationService<NhUserAccount> authenticationService,
            ISession session,
            UserAccountService<NhUserAccount> userAccountService)
        {
            this.authenticationService = authenticationService;
            this.session = session;
            this.userAccountService = userAccountService;
        }

        public ActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Register(RegistrationViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            NhUserAccount account;
            using (var tx = this.session.BeginTransaction())
            {
                account = this.userAccountService.CreateAccount(model.Username, model.Password, model.Email);
                tx.Commit();
            }

            if (account == null)
            {
                return this.View();
            }

            this.authenticationService.SignIn(account);
            return this.RedirectToAction("Index", "Home");
        }

        public ActionResult Login()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            NhUserAccount account;
            using (var tx = this.session.BeginTransaction())
            {
                if (this.userAccountService.AuthenticateWithUsernameOrEmail(model.Username, model.Password, out account))
                {
                    this.authenticationService.SignIn(account, model.RememberMe);

                    if (this.userAccountService.IsPasswordExpired(account))
                    {
                        return this.RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        if (this.Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return this.Redirect(model.ReturnUrl);
                        }
                        else
                        {
                            return this.RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    this.ModelState.AddModelError(string.Empty, "Invalid Username or Password");
                }

                tx.Commit();
            }

            return this.RedirectToAction("Index", "Home");
        }

        public ActionResult SignOut()
        {
            this.authenticationService.SignOut();
            return this.RedirectToAction("Index", "Home");
        }
    }
}