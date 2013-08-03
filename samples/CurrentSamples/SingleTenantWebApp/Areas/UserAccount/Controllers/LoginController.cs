using System.Web.Mvc;
using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        UserAccountService userAccountService;
        ClaimsBasedAuthenticationService authSvc;

        public LoginController(
            UserAccountService userService, 
            ClaimsBasedAuthenticationService authSvc)
        {
            this.userAccountService = userService;
            this.authSvc = authSvc;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.userAccountService != null)
                {
                    this.userAccountService.Dispose();
                    this.userAccountService = null;
                }
                
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

                    if (account.UseTwoFactorAuth)
                    {
                        return View("TwoFactorAuth");
                    }
                    
                    if (userAccountService.IsPasswordExpired(account.Username))
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
            if (button == "signin")
            {
                if (ModelState.IsValid)
                {
                    BrockAllen.MembershipReboot.UserAccount account;
                    if (userAccountService.AuthenticateWithCode(this.User.GetUserID(), model.Code, out account))
                    {
                        authSvc.SignIn(account);

                        if (userAccountService.IsPasswordExpired(account.Username))
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
    }
}
