using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class PasswordResetController : Controller
    {
        UserAccountService<CustomUserAccount> userAccountService;
        public PasswordResetController(UserAccountService<CustomUserAccount> userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        public ActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(PasswordResetInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.userAccountService.ResetPassword(model.Email);
                    return View("ResetSuccess");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View();
        }

        public ActionResult Confirm(string id)
        {
            var vm = new ChangePasswordFromResetKeyInputModel()
            {
                Key = id
            };
            return View("Confirm", vm);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(ChangePasswordFromResetKeyInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (this.userAccountService.ChangePasswordFromResetKey(model.Key, model.Password))
                    {
                        return View("Success");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error changing password. The key might be invalid.");
                    }
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View();
        }
    }
}
