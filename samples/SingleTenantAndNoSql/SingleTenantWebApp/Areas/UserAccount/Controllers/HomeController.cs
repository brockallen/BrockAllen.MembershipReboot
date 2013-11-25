using System;
using System.Security.Claims;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class HomeController : Controller
    {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public HomeController(
            UserAccountService userAccountService, AuthenticationService authSvc)
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
                userAccountService.RemoveClaim(User.GetUserID(), ClaimTypes.Gender);
            }
            else
            {
                // if you only want one of these claim types, uncomment the next line
                //account.RemoveClaim(ClaimTypes.Gender);
                userAccountService.AddClaim(User.GetUserID(), ClaimTypes.Gender, gender);
            }
            userAccountService.Update(account);

            // since we've changed the claims, we need to re-issue the cookie that
            // contains the claims.
            authSvc.SignIn(account);

            return RedirectToAction("Index");
        }

    }
}
