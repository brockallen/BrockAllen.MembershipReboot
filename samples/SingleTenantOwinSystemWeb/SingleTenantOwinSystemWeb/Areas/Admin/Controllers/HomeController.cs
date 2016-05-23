using BrockAllen.MembershipReboot.Ef;
using BrockAllen.MembershipReboot.Relational;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        UserAccountService<RelationalUserAccount> userAccountService;
        IUserAccountQuery<RelationalUserAccount> query;

        public HomeController(UserAccountService<RelationalUserAccount> userAccountService)
        {
            this.userAccountService = userAccountService;
            this.query = userAccountService.Query;
        }

        public ActionResult Index(string filter)
        {
            int count;
            var accounts = query.Query(list =>
            {
                if (filter != null)
                {
                    list = list.Where(x => x.Username.Contains(filter));
                }
                return list;
            },
            null,
            //list => list.OrderBy(x=>x.Username),
            0, 1000, out count);

            return View("Index", accounts.ToArray());
        }

        public ActionResult Detail(Guid id)
        {
            var account = userAccountService.GetByID(id);
            return View("Detail", account);
        }

        [HttpPost]
        public ActionResult Reopen(Guid id)
        {
            try
            {
                userAccountService.ReopenAccount(id);
                return RedirectToAction("Detail", new { id });
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Detail(id);
        }
    }
}
