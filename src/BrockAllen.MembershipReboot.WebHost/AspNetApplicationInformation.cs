/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Web;
namespace BrockAllen.MembershipReboot.WebHost
{
    public class AspNetApplicationInformation : RelativePathApplicationInformation
    {
        public AspNetApplicationInformation(
            string appName,
            string emailSig,
            string relativeLoginUrl,
            string relativeConfirmPasswordResetUrl,
            string relativeConfirmChangeEmailUrl,
            string relativeCancelVerificationUrl)
        {
            this.ApplicationName = appName;
            this.EmailSignature = emailSig;
            this.RelativeLoginUrl = relativeLoginUrl;
            this.RelativeConfirmPasswordResetUrl = relativeConfirmPasswordResetUrl;
            this.RelativeConfirmChangeEmailUrl = relativeConfirmChangeEmailUrl;
            this.RelativeCancelVerificationUrl = relativeCancelVerificationUrl;
        }

        protected override string GetApplicationBaseUrl()
        {
            return HttpContext.Current.GetApplicationUrl();
        }
    }
}
