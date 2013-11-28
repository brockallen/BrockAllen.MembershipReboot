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
                    var account = this.userAccountService.CreateAccount(model.Username, model.Password, model.Email);
                    if (userAccountService.Configuration.RequireAccountVerification)
                    {
                        return View("Success", model);
                    }
                    else
                    {
                        authSvc.SignIn(account);
                        return RedirectToAction("ConfirmResult", new { success = true });
                    }
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(model);
        }

        public ActionResult Verify()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Verify(string foo)
        {
            try
            {
                this.userAccountService.RequestAccountVerification(User.GetUserID());
                return View("Success");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View();
        }

        public ActionResult Confirm(string id)
        {
            return View("Confirm");
        }

        [HttpPost]
        public ActionResult Confirm(string id, string password)
        {
            BrockAllen.MembershipReboot.UserAccount account;
            this.userAccountService.VerifyEmailFromKey(id, password, out account);
            authSvc.SignIn(account);

            return RedirectToAction("ConfirmResult", new { success = true });
        }

        public ActionResult ConfirmResult(bool success)
        {
            return View("ConfirmResult", success);
        }

        public ActionResult Cancel(string id)
        {
            try
            {
                this.userAccountService.CancelNewAccount(id);
            }
            catch(ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View("Cancel");
        }
    }
}
