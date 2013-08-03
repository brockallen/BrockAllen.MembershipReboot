using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [Authorize]
    public class ChangeMobileController : Controller
    {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public ChangeMobileController(UserAccountService userAccountService, AuthenticationService authSvc)
        {
            this.userAccountService = userAccountService;
            this.authSvc = authSvc;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.userAccountService.TryDispose();
                this.userAccountService = null;
            }
            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string button, ChangeMobileRequestInputModel model)
        {
            if (button == "change")
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        if (this.userAccountService.ChangeMobilePhoneRequest(User.Identity.Name, model.NewMobilePhone))
                        {
                            return View("ChangeRequestSuccess", (object)model.NewMobilePhone);
                        }

                        ModelState.AddModelError("", "Error requesting mobile phone number change.");
                    }
                    catch (ValidationException ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }

            if (button == "remove")
            {
                if (this.userAccountService.RemoveMobilePhone(User.GetUserID()))
                {
                    return View("Success");
                }
                else
                {
                    ModelState.AddModelError("", "Error removing the mobile phone");
                }
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Confirm(ChangeMobileFromCodeInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (this.userAccountService.ChangeMobileFromCode(this.User.Identity.Name, model.Code))
                    {
                        // since the mobile had changed, reissue the 
                        // cookie with the updated claims
                        authSvc.SignIn(this.User.Identity.Name);

                        return View("Success");
                    }

                    ModelState.AddModelError("", "Error confirming code.");
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
