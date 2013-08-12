using BrockAllen.MembershipReboot;
using BrockAllen.OAuth2;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace LinkedAccounts.Controllers
{
    public class HomeController : Controller
    {
        static HomeController()
        {
            RegisterOAuth2Clients();
        }

        static void RegisterOAuth2Clients()
        {
            OAuth2Client.Instance.RegisterProvider(
                ProviderType.Google,
                "464251281574.apps.googleusercontent.com",
                "najvdnYI5TjCkikCi6nApRu1");

            OAuth2Client.Instance.RegisterProvider(
                ProviderType.Facebook,
                "260581164087472",
                "7389d78e6e629954a710351830d080f3");

            //OAuth2Client.Instance.RegisterProvider(
            //    ProviderType.Live,
            //    "00000000400DF045",
            //    "4L08bE3WM8Ra4rRNMv3N--un5YOBr4gx");
        }

        AuthenticationService AuthenticationService;
        UserAccountService userAccountService;

        public HomeController(
            AuthenticationService AuthenticationService)
        {
            this.AuthenticationService = AuthenticationService;
            this.userAccountService = AuthenticationService.UserAccountService;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.AuthenticationService.TryDispose();
                this.AuthenticationService = null;
            }

            base.Dispose(disposing);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                AuthenticationService.SignOut();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Login(ProviderType type)
        {
            return new OAuth2ActionResult(type);
        }

        public async Task<ActionResult> OAuthCallback()
        {
            try
            {
                var result = await OAuth2Client.Instance.ProcessCallbackAsync();
                if (result.Error == null)
                {
                    var provider = result.ProviderName;
                    var claims = result.Claims;
                    var id = claims.GetValue(ClaimTypes.NameIdentifier);

                    this.AuthenticationService.SignInWithLinkedAccount(provider, id, claims);

                    if (result.ReturnUrl != null)
                    {
                        return Redirect(result.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ModelState.AddModelError("", result.Error);
                    if (!String.IsNullOrWhiteSpace(result.ErrorDetails))
                    {
                        ModelState.AddModelError("", result.ErrorDetails);
                    }
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

            return View("SignInError");
        }

        public ActionResult Confirm(string id)
        {
            var result = this.userAccountService.VerifyAccount(id);
            return View("Index");
        }

        public ActionResult Cancel(string id)
        {
            var result = this.userAccountService.CancelNewAccount(id);
            return View("Index");
        }

        public ActionResult CloseAccount()
        {
            this.userAccountService.DeleteAccount(User.GetUserID());
            this.AuthenticationService.SignOut();
            return RedirectToAction("Index");
        }
    }
}
