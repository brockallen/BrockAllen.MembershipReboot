/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.AspNet.Http;
using HttpContext = System.Web.HttpContext;

namespace BrockAllen.MembershipReboot.WebHost
{
    public class AspNetApplicationInformation : RelativePathApplicationInformation
    {
        public IHttpContextAccessor ContextAccessor { get; set; }

        public AspNetApplicationInformation(
            IHttpContextAccessor contextAccessor,
            string appName,
            string emailSig,
            string relativeLoginUrl,
            string relativeConfirmChangeEmailUrl,
            string relativeCancelVerificationUrl,
            string relativeConfirmPasswordResetUrl
        )
        {
            this.ContextAccessor = contextAccessor;
            this.ApplicationName = appName;
            this.EmailSignature = emailSig;
            this.RelativeLoginUrl = relativeLoginUrl;
            this.RelativeConfirmChangeEmailUrl = relativeConfirmChangeEmailUrl;
            this.RelativeCancelVerificationUrl = relativeCancelVerificationUrl;
            this.RelativeConfirmPasswordResetUrl = relativeConfirmPasswordResetUrl;
        }

        protected override string GetApplicationBaseUrl()
        {
            return HttpContext.Current.GetApplicationUrl();
        }
    }
}
