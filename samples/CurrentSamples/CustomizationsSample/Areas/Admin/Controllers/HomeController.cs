using System;
using System.Linq;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        IUserAccountRepository userAccountRepository;
        public HomeController(IUserAccountRepository userAccountRepository)
        {
            this.userAccountRepository = userAccountRepository;
        }

        public ActionResult Index()
        {
            var names =
                from a in userAccountRepository.GetAll()
                select a;
            return View(names.ToArray());
        }

        public ActionResult Detail(Guid id)
        {
            var account = userAccountRepository.Get(id);
            return View("Detail", account);
        }

    }
}
