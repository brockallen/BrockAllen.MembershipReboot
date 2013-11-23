using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [Authorize]
    public class ChangeMobileController : Controller
    {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public ChangeMobileController(AuthenticationService authSvc)
        {
            this.userAccountService = authSvc.UserAccountService;
            this.authSvc = authSvc;
        }

        public ActionResult Index()
        {
            var acct = this.authSvc.UserAccountService.GetByID(User.GetUserID());
            return View("Index", new ChangeMobileRequestInputModel {Current = acct.MobilePhoneNumber });
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
                        this.userAccountService.ChangeMobilePhoneRequest(User.GetUserID(), model.NewMobilePhone);
                        return View("ChangeRequestSuccess", (object)model.NewMobilePhone);
                    }
                    catch (ValidationException ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }

            if (button == "remove")
            {
                this.userAccountService.RemoveMobilePhone(User.GetUserID());
                return View("Success");
            }

            var acct = this.authSvc.UserAccountService.GetByID(User.GetUserID());
            model.Current = acct.MobilePhoneNumber;
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
                    if (this.userAccountService.ChangeMobilePhoneFromCode(this.User.GetUserID(), model.Code))
                    {
                        // since the mobile had changed, reissue the 
                        // cookie with the updated claims
                        authSvc.SignIn(User.GetUserID());

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
