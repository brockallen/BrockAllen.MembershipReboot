using BrockAllen.MembershipReboot.Ef;
using System;
using System.Linq;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        CustomDatabase db;
        public HomeController()
        {
            this.db = new CustomDatabase();
        }

        public ActionResult Index()
        {
            var names =
                from a in db.UserAccountsTableWithSomeOtherName
                select a;
            return View(names.ToArray());
        }

        public ActionResult Detail(Guid id)
        {
            var account = db.UserAccountsTableWithSomeOtherName.Find(id);
            return View("Detail", account);
        }

    }
}
