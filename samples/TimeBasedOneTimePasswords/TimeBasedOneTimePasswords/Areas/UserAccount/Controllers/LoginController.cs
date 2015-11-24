using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using BrockAllen.MembershipReboot;
using Owin;
using TimeBasedOneTimePasswords.Areas.UserAccount.Models;

namespace TimeBasedOneTimePasswords.Areas.UserAccount.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        UserAccountService userAccountService;
        AuthenticationService authSvc;

        public LoginController(AuthenticationService authSvc)
        {
            this.userAccountService = authSvc.UserAccountService;
            this.authSvc = authSvc;
        }

        public ActionResult Index()
        {
            return View(new LoginInputModel());
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                BrockAllen.MembershipReboot.UserAccount account;
                if (userAccountService.AuthenticateWithUsernameOrEmail(model.Username, model.Password, out account))
                {
                    authSvc.SignIn(account, model.RememberMe);

                    if (account.RequiresTwoFactorAuthCodeToSignIn())
                    {
                        return RedirectToAction("TwoFactorAuthCodeLogin");
                    }
                    if (account.RequiresRFC6238CodeToSignIn())
                    {
                        return RedirectToAction("RFC6238CodeLogin");
                    }
                    if (account.RequiresTwoFactorCertificateToSignIn())
                    {
                        return RedirectToAction("CertificateLogin");
                    }

                    if (account.RequiresPasswordReset)
                    {
                        // this might mean many things -- 
                        // it might just mean that the user should change the password, 
                        // like the expired password below, so we'd just redirect to change password page
                        // or, it might mean the DB was compromised, so we want to force the user
                        // to reset their password but via a email token, so we'd want to 
                        // let the user know this and invoke ResetPassword and not log them in
                        // until the password has been changed
                        //userAccountService.ResetPassword(account.ID);

                        // so what you do here depends on your app and how you want to define the semantics
                        // of the RequiresPasswordReset property
                    }
                    
                    if (userAccountService.IsPasswordExpired(account))
                    {
                        return RedirectToAction("Index", "ChangePassword");
                    }
                    
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid Username or Password");
                }
            }

            return View(model);
        }

        public ActionResult TwoFactorAuthCodeLogin()
        {
            var ctx = Request.GetOwinContext();
            var id = ctx.GetIdFromTwoFactorCookie();
            if (id == null)
            {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TwoFactorAuthCodeLogin(string button, TwoFactorAuthInputModel model)
        {
            var ctx = Request.GetOwinContext();
            var id = ctx.GetIdFromTwoFactorCookie();
            if (id == null)
            {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }

            if (button == "signin")
            {
                if (ModelState.IsValid)
                {
                    BrockAllen.MembershipReboot.UserAccount account;
                    if (userAccountService.AuthenticateWithCode(id.Value, model.Code, out account))
                    {
                        authSvc.SignIn(account);

                        if (userAccountService.IsPasswordExpired(account))
                        {
                            return RedirectToAction("Index", "ChangePassword");
                        }

                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid Code");
                    }
                }
            }
            
            if (button == "resend")
            {
                ModelState.Clear();
                this.userAccountService.SendTwoFactorAuthenticationCode(this.User.GetUserID());
            }

            return View("TwoFactorAuthCodeLogin", model);
        }

        public ActionResult CertificateLogin()
        {
            if (Request.ClientCertificate != null && 
                Request.ClientCertificate.IsPresent && 
                Request.ClientCertificate.IsValid)
            {
                try
                {
                    var cert = new X509Certificate2(Request.ClientCertificate.Certificate);
                    BrockAllen.MembershipReboot.UserAccount account;

                    var result = false;
                    // we're allowing the use of certs for login and for two factor auth. normally you'd 
                    // do only one or the other, but for the sake of the sample we're allowing both.
                    var ctx = Request.GetOwinContext();
                    var id = ctx.GetIdFromTwoFactorCookie();
                    if (id != null)
                    {
                        // this is when we're doing cert logins for two factor auth
                        result = this.authSvc.UserAccountService.AuthenticateWithCertificate(id.Value, cert, out account);
                    }
                    else
                    {
                        // this is when we're just doing certs to login (so no two factor auth)
                        result = this.authSvc.UserAccountService.AuthenticateWithCertificate(cert, out account);
                    }

                    if (result)
                    {
                        this.authSvc.SignIn(account, AuthenticationMethods.X509);

                        if (userAccountService.IsPasswordExpired(account))
                        {
                            return RedirectToAction("Index", "ChangePassword");
                        }

                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError("", "Invalid login");
                }
                catch (ValidationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
            return View();
        }

        public ActionResult Rfc6238CodeLogin() {
            var ctx = Request.GetOwinContext();
            var id = ctx.GetIdFromTwoFactorCookie();
            if (id == null)
            {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rfc6238CodeLogin(string button, TwoFactorAuthInputModel model)
        {
            var ctx = Request.GetOwinContext();
            var id = ctx.GetIdFromTwoFactorCookie();
            if (id == null)
            {
                // if the temp cookie is expired, then make the login again
                return RedirectToAction("Index");
            }

            if (button == "signin")
            {
                if (ModelState.IsValid)
                {
                    BrockAllen.MembershipReboot.UserAccount account;
                    if (userAccountService.AuthenticateWithAutenticatorCode(id.Value, model.Code, out account))
                    {
                        authSvc.SignIn(account);

                        if (userAccountService.IsPasswordExpired(account))
                        {
                            return RedirectToAction("Index", "ChangePassword");
                        }

                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid Code");
                    }
                }
            }

            return View("Rfc6238CodeLogin", model);
        }
    }
}
