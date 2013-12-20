using BrockAllen.MembershipReboot.Hierarchical;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class LogoutController : Controller
    {
        AuthenticationService<HierarchicalUserAccount> authSvc;
        public LogoutController(AuthenticationService<HierarchicalUserAccount> authSvc)
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
