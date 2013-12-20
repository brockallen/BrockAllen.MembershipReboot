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
                    ViewData["RequireAccountVerification"] = this.userAccountService.Configuration.RequireAccountVerification;
                    return View("Success", model);
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

        public ActionResult Cancel(string id)
        {
            try
            {
                bool closed;
                this.userAccountService.CancelNewAccount(id, out closed);
                if (closed)
                {
                    return View("Closed");
                }
                else
                {
                    return View("Cancel");
                }
            }
            catch(ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View("Error");
        }

        [HttpGet]
        [Authorize]
        public ActionResult External()
        {
            var UserInfo=this.userAccountService.GetByID(User.GetUserID());
            return View("Index", new RegisterInputModel() { Username = UserInfo.Username, Email = UserInfo.Email, Password = "", ConfirmPassword = "" });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult External(RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    var account = this.userAccountService.GetByID(User.GetUserID());
                    this.userAccountService.UpdateExternal(account, model.Username, model.Password, model.Email);
                    ViewData["RequireAccountVerification"] = this.userAccountService.Configuration.RequireAccountVerification;
                    return View("Success", model);
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                
            }
            return View("Index");
        }
    }
}
