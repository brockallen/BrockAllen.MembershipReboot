using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        UserAccountService userService;
        ClaimsBasedAuthenticationService authSvc;

        public LoginController(
            UserAccountService userService, 
            ClaimsBasedAuthenticationService authSvc)
        {
            this.userService = userService;
            this.authSvc = authSvc;
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
                if (this.userService.Authenticate(model.Username, model.Password))
                {
                    authSvc.SignIn(model.Username);
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Username or Password");
                }
            }

            return View(model);
        }
    }
}
