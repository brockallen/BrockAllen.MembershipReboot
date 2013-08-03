using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class TwoFactorAuthController : Controller
    {
        UserAccountService userAccountService;

        public TwoFactorAuthController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }
        
        public ActionResult Index()
        {
            var acct = userAccountService.GetByUsername(this.User.Identity.Name);
            return View(acct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Enable()
        {
            if (this.userAccountService.EnableTwoFactorAuthentication(this.User.GetUserID()))
            {
                return View("Success");
            }

            return View("Fail");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disable()
        {
            this.userAccountService.DisableTwoFactorAuthentication(this.User.GetUserID());
            return View("Success");
        }
    }
}
