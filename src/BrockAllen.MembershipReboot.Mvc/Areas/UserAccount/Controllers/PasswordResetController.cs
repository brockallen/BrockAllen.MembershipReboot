using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class PasswordResetController : Controller
    {
        UserAccountService userAccountService;
        public PasswordResetController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.userAccountService != null)
                {
                    this.userAccountService.Dispose();
                    this.userAccountService = null;
                }
            }
            base.Dispose(disposing);
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
                    if (this.userAccountService.ResetPasswordWithEmail(model.Email))
                    {
                        return View("ResetSuccess");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error resetting password. The email might be invalid.");
                    }
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
