using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [Authorize]
    public class ChangePasswordController : Controller
    {
        UserAccountService userAccountService;
        public ChangePasswordController(UserAccountService userAccountService)
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
            return View(new ChangePasswordInputModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ChangePasswordInputModel model)
        {
            if (ModelState.IsValid)
            {
                if (this.userAccountService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return View("Success");
                }

                ModelState.AddModelError("", "Error changing password");
            }
            return View(model);
        }
    }
}
