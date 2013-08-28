using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class LogoutController : Controller
    {
        AuthenticationService authSvc;
        public LogoutController(AuthenticationService authSvc)
        {
            this.authSvc = authSvc;
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
