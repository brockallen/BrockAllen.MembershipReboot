using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [Authorize]
    public class CloseAccountController : Controller
    {
        UserAccountService userAccountService;
        public CloseAccountController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.userAccountService.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string button)
        {
            if (button == "yes")
            {
                if (this.userAccountService.DeleteAccount(User.Identity.Name))
                {
                    return RedirectToAction("Index", "Logout");
                }
                ModelState.AddModelError("", "Error closing your account");
            }

            return View();
        }

    }
}
