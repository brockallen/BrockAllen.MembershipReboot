using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    //[Authorize]
    public class ChangePasswordController : Controller
    {
        UserAccountService userAccountService;
        public ChangePasswordController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }
        
        public ActionResult Index()
        {
            if (!User.HasUserID())
            {
                return new HttpUnauthorizedResult();
            }
            return View(new ChangePasswordInputModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ChangePasswordInputModel model)
        {
            if (!User.HasUserID())
            {
                return new HttpUnauthorizedResult();
            }

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
