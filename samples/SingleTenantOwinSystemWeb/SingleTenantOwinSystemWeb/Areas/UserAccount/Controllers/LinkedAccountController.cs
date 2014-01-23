using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BrockAllen.MembershipReboot.Mvc.Areas.UserAccount.Controllers
{
    public class LinkedAccountController : Controller
    {
        AuthenticationService authenticationService;
        UserAccountService userAccountService;

        public LinkedAccountController(
            AuthenticationService AuthenticationService)
        {
            this.authenticationService = AuthenticationService;
            this.userAccountService = AuthenticationService.UserAccountService;
        }

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public void Login(string type)
        {
            var ctx = Request.GetOwinContext();
            var props = new AuthenticationProperties{ 
                RedirectUri=Url.Action("OAuthCallback")
            };
            props.Dictionary.Add("Provider", type);
            ctx.Authentication.Challenge(props, type);
        }

        [Authorize]
        public ActionResult Manage()
        {
            var linkedAccounts = this.userAccountService.GetByID(User.GetUserID()).LinkedAccounts.ToArray();
            return View("Manage", linkedAccounts);
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(string provider, string id)
        {
            try
            {
                this.userAccountService.RemoveLinkedAccount(User.GetUserID(), provider, id);
                return RedirectToAction("Manage");
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Manage();
        }

        [AllowAnonymous]
        public async Task<ActionResult> OAuthCallback()
        {
            try
            {
                var ctx = Request.GetOwinContext();
                var result = await ctx.Authentication.AuthenticateAsync("External");
                if (result == null || result.Identity == null)
                {
                    ModelState.AddModelError("", "Error on callback");
                }
                else
                {
                    var provider = result.Properties.Dictionary["Provider"];
                    var claims = result.Identity.Claims;
                    var id = claims.GetValue(ClaimTypes.NameIdentifier);

                    BrockAllen.MembershipReboot.UserAccount account;
                    this.authenticationService.SignInWithLinkedAccount(provider, id, claims, out account);

                    if (!account.IsAccountVerified && userAccountService.Configuration.RequireAccountVerification)
                    {
                        return View("NotLoggedIn", account);
                    }
                    
                    ctx.Authentication.SignOut("External");
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (ValidationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error Signing In");
            }

            //return View("~/Areas/UserAccount/Views/LinkedAccount/SignInError.cshtml");
            return View("SignInError");
        }
    }
}
