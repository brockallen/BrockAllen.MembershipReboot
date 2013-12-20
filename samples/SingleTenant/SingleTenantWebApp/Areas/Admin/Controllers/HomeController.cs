using BrockAllen.MembershipReboot.Ef;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        DefaultMembershipRebootDatabase db;
        UserAccountService userAccountService;

        public HomeController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
            this.db = new DefaultMembershipRebootDatabase();
        }

        public ActionResult Index()
        {
            var names =
                from a in db.Users
                select a;
            return View("Index", names.ToArray());
        }

        [HttpPost]
        public ActionResult Reopen(Guid id)
        {
            try
            {
                userAccountService.ReopenAccount(id);
                return RedirectToAction("Index");
            }
            catch(ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Index();
        }

        public ActionResult Detail(Guid id)
        {
            var account = db.Users.Find(id);
            return View("Detail", account);
        }

    }
}
