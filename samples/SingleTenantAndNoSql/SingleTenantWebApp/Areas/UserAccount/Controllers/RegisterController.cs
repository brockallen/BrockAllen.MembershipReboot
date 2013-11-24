using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public RegisterController(AuthenticationService authSvc)
        {
            this.authSvc = authSvc;
            this.userAccountService = authSvc.UserAccountService;
        }

        public ActionResult Index()
        {
            return View(new RegisterInputModel());
        }
        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Index(RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.userAccountService.CreateAccount(model.Username, model.Password, model.Email);
                    if (userAccountService.Configuration.SecuritySettings.RequireAccountVerification)
                    {
                        return View("Success", model);
                    }
                    else
                    {
                        return View("Confirm", true);
                    }
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult Confirm(string id)
        {
            return View("Confirm");
        }

        [HttpPost]
        public ActionResult Confirm(string id, string password)
        {
            BrockAllen.MembershipReboot.UserAccount account;
            var result = this.userAccountService.VerifyAccount(id, password, out account);
            if (result)
            {
                authSvc.SignIn(account);
            }

            return RedirectToAction("ConfirmResult", new { success = result });
        }

        public ActionResult ConfirmResult(bool success)
        {
            return View("ConfirmResult", success);
        }

        public ActionResult Cancel(string id)
        {
            var result = this.userAccountService.CancelNewAccount(id);
            return View("Cancel", result);
        }
    }
}
