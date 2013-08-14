﻿using System.ComponentModel.DataAnnotations;
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
                if (this.userAccountService != null)
                {
                    this.userAccountService.Dispose();
                    this.userAccountService = null;
                }
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
                try
                {
                    this.userAccountService.DeleteAccount(User.GetUserID());
                    return RedirectToAction("Index", "Logout");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View();
        }

    }
}
