using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [Authorize]
    public class ChangeEmailController : Controller
    {
        UserAccountService<CustomUserAccount> userAccountService;
        AuthenticationService<CustomUserAccount> authSvc;

        public ChangeEmailController(
            UserAccountService<CustomUserAccount> userAccountService, AuthenticationService<CustomUserAccount> authSvc)
        {
            this.userAccountService = userAccountService;
            this.authSvc = authSvc;
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ChangeEmailRequestInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    this.userAccountService.ChangeEmailRequest(User.GetUserID(), model.NewEmail);
                    return View("ChangeRequestSuccess", (object)model.NewEmail);
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View("Index", model);
        }

        public ActionResult Confirm(string id)
        {
            var vm = new ChangeEmailFromKeyInputModel();
            vm.Key = id;
            return View("Confirm", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(ChangeEmailFromKeyInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (this.userAccountService.ChangeEmailFromKey(User.GetUserID(), model.Password, model.Key))
                    {
                        // since we've changed the email, we need to re-issue the cookie that
                        // contains the claims.
                        var account = this.userAccountService.GetByID(User.GetUserID());
                        authSvc.SignIn(account);
                        return View("Success");
                    }

                    ModelState.AddModelError("", "Error changing email.");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            return View("Confirm", model);
        }
    }
}
