using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class LogoutController : Controller
    {
        ClaimsBasedAuthenticationService authSvc;
        public LogoutController(ClaimsBasedAuthenticationService authSvc)
        {
            this.authSvc = authSvc;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.authSvc.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                authSvc.SignOut();
                return RedirectToAction("Index");
            }
            
            return View();
        }

    }
}
