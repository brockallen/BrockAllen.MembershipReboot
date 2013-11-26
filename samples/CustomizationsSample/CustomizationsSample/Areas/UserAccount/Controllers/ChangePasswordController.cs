using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [Authorize]
    public class ChangePasswordController : Controller
    {
        UserAccountService<CustomUserAccount> userAccountService;
        public ChangePasswordController(UserAccountService<CustomUserAccount> userAccountService)
        {
            this.userAccountService = userAccountService;
        }
        
        public ActionResult Index()
        {
            return View(new ChangePasswordInputModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ChangePasswordInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.userAccountService.ChangePassword(User.GetUserID(), model.OldPassword, model.NewPassword);
                    return View("Success");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }
    }
}
