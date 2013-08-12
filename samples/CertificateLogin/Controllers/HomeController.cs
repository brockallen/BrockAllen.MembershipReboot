using BrockAllen.MembershipReboot;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;

namespace CertificateLogin.Controllers
{
    public class HomeController : Controller
    {
        AuthenticationService authenticationService;
        public HomeController(AuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Logout()
        {
            authenticationService.SignOut();
            return RedirectToAction("Index");
        }
        
        public ActionResult Cert()
        {
            if (Request.ClientCertificate != null &&
                Request.ClientCertificate.IsPresent &&
                Request.ClientCertificate.IsValid)
            {
                var cert = new X509Certificate2(Request.ClientCertificate.Certificate);
                UserAccount account;
                if (this.authenticationService.UserAccountService.AuthenticateWithCertificate(cert, out account))
                {
                    this.authenticationService.SignIn(account, AuthenticationMethods.X509);
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Invalid login");
            }
            
            return View("Index");
        }
    }
}
