using BrockAllen.MembershipReboot.Hierarchical;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class TwoFactorAuthController : Controller
    {
        UserAccountService<HierarchicalUserAccount> userAccountService;

        public TwoFactorAuthController(UserAccountService<HierarchicalUserAccount> userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        public ActionResult Index()
        {
            var acct = userAccountService.GetByID(this.User.GetUserID());
            return View(acct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(TwoFactorAuthMode mode)
        {
            try
            {
                this.userAccountService.ConfigureTwoFactorAuthentication(this.User.GetUserID(), mode);
                
                ViewData["Message"] = "Update Success";
                
                var acct = userAccountService.GetByID(this.User.GetUserID());
                return View("Index", acct);
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            
            return View("Index", userAccountService.GetByID(this.User.GetUserID()));
        }
    }
}
