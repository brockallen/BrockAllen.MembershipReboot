using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /UserAccount/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
