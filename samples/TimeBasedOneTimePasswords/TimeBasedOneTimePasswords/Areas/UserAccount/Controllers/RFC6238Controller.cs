using System.Web.Mvc;
using BrockAllen.MembershipReboot;
using TimeBasedOneTimePasswords.Areas.UserAccount.Models;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Controllers
{
    public class RFC6238Controller : Controller
    {
        UserAccountService userAccountService;

        public RFC6238Controller(UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }

        // GET: UserAccount/RFC6238
        public ActionResult Index()
        {
            BrockAllen.MembershipReboot.UserAccount account = userAccountService.GetByID(this.User.GetUserID());
            return View("Index", new RFC6238Model
            {
                Account = account,
                IsError = false,
            });
        }

        [HttpGet]
        public ActionResult Activate()
        {
            var isError = false;
            var errorMessage = string.Empty;
            BrockAllen.MembershipReboot.UserAccount account = userAccountService.GetByID(this.User.GetUserID());
            if (account != null)
            {
                if (account.AuthenticatorActive)
                {
                    isError = true;
                    errorMessage = "Authenticator already active";
                }
                userAccountService.ResetAuthenticatorSecret(account);
            }
            return View("Index", new RFC6238Model
            {
                Account = account,
                IsError = isError,
                Message = errorMessage,
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Activate(string two_factor_key)
        {
            var isError = false;
            var errorMessage = string.Empty;
            BrockAllen.MembershipReboot.UserAccount account = userAccountService.GetByID(this.User.GetUserID());
            if (account != null)
            {
                if (account.AuthenticatorActive)
                {
                    isError = true;
                    errorMessage = "Authenticator already active";
                }
                else
                {
                    userAccountService.ValidateAuthenticatorSecret(account, two_factor_key);
                    isError = !account.AuthenticatorActive;
                    if (isError)
                    {
                        errorMessage = "Failed to validate authenticate token";
                    }
                }
            }
            if (isError)
            {
                return View("Index", new RFC6238Model
                {
                    Account = account,
                    IsError = isError,
                    Message = errorMessage
                });
            }
            return this.RedirectToAction("Index");
        }
    }
}