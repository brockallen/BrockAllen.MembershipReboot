using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        UserAccountService userAccountService;
        public HomeController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        public ActionResult Index()
        {
            var names =
                from a in userAccountService.GetAll()
                select a;
            return View(names.ToArray());
        }

        public ActionResult Detail(int id)
        {
            var account = userAccountService.GetByID(id);
            return View("Detail", account);
        }

    }
}
