using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public LoginController(AuthenticationService authSvc)
        {
            this.userAccountService = authSvc.UserAccountService;
            this.authSvc = authSvc;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.authSvc != null)
                {
                    this.authSvc.Dispose();
                    this.authSvc = null;
                }
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View(new LoginInputModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                BrockAllen.MembershipReboot.UserAccount account;
                if (userAccountService.AuthenticateWithUsernameOrEmail(model.Username, model.Password, out account))
                {
                    authSvc.SignIn(account);

                    if (account.RequiresTwoFactorAuthCodeToSignIn)
                    {
                        return View("TwoFactorAuth");
                    }
                    
                    if (userAccountService.IsPasswordExpired(account))
                    {
                        return RedirectToAction("Index", "ChangePassword");
                    }
                    
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Username or Password");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwoFactorAuthLogin(string button, TwoFactorAuthInputModel model)
        {
            if (!User.HasUserID())
            {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }

            if (button == "signin")
            {
                if (ModelState.IsValid)
                {
                    BrockAllen.MembershipReboot.UserAccount account;
                    if (userAccountService.AuthenticateWithCode(this.User.GetUserID(), model.Code, out account))
                    {
                        authSvc.SignIn(account);

                        if (userAccountService.IsPasswordExpired(account))
                        {
                            return RedirectToAction("Index", "ChangePassword");
                        }

                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid Code");
                    }
                }
            }
            
            if (button == "resend")
            {
                this.userAccountService.SendTwoFactorAuthenticationCode(this.User.GetUserID());
            }

            return View("TwoFactorAuth", model);
        }

        public ActionResult LoginWithCertificate()
        {
            if (Request.ClientCertificate != null)
            {

            }
            else
            {
                ModelState.AddModelError("", "No certificate provided");
            }
            return View();
        }
    }
}
