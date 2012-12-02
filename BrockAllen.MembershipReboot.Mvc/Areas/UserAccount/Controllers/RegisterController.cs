using BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        UserAccountService userAccountService;
        
        public RegisterController(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }
        
        public ActionResult Index(string returnUrl)
        {
            try
            {
                if (returnUrl != null)
                {
                    if (!Url.IsLocalUrl(returnUrl))
                    {
                        var url = new Uri(returnUrl, UriKind.Absolute);
                    }
                }
            }
            catch
            {
                returnUrl = null;
                ModelState.AddModelError("", "ReturnUrl is not a valid URL. It was ignored.");
            }

            var vm = new RegisterInputModel()
            {
                ReturnUrl = returnUrl
            };
            return View(vm);
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
                    return View("Success", model);
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Error creating account.");
                }
            }
            return View();
        }

        public ActionResult Confirm(string id)
        {
            var result = this.userAccountService.VerifyAccount(id);
            return View("Confirm", result);
        }
        
        public ActionResult Cancel(string id)
        {
            var result = this.userAccountService.CancelNewAccount(id);
            return View("Cancel", result);
        }
    }
}
