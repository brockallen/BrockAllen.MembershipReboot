using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "UserAccount" });
        }

    }
}
