using BrockAllen.MembershipReboot.Ef;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        DefaultMembershipRebootDatabase db;
        public HomeController()
        {
            this.db = new DefaultMembershipRebootDatabase();
        }

        public ActionResult Index()
        {
            var names =
                from a in db.Users
                select a;
            return View(names.ToArray());
        }

        public ActionResult Detail(Guid id)
        {
            var account = db.Users.Find(id);
            return View("Detail", account);
        }

    }
}
