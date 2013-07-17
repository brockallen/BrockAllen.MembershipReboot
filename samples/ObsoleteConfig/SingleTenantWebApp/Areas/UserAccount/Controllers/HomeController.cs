using System;
using System.Security.Claims;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class HomeController : Controller
    {
        UserAccountService userAccountService;
        ClaimsBasedAuthenticationService authSvc;

        public HomeController(
            UserAccountService userAccountService, ClaimsBasedAuthenticationService authSvc)
        {
            this.userAccountService = userAccountService;
            this.authSvc = authSvc;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Index(string gender)
        {
            var account = userAccountService.GetByUsername(User.Identity.Name);
            if (String.IsNullOrWhiteSpace(gender))
            {
                account.RemoveClaim(ClaimTypes.Gender);
            }
            else
            {
                // if you only want one of these claim types, uncomment the next line
                //account.RemoveClaim(ClaimTypes.Gender);
                account.AddClaim(ClaimTypes.Gender, gender);
            }
            userAccountService.Update(account);

            // since we've changed the claims, we need to re-issue the cookie that
            // contains the claims.
            authSvc.SignIn(User.Identity.Name);


            return RedirectToAction("Index");
        }

    }
}
