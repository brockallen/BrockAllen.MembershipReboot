using System.Web.Mvc;

namespace TimeBasedOneTimePasswords.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "UserAccount" });
        }

    }
}
