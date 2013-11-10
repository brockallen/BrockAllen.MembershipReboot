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
            string relativeVerifyAccountUrl,
            string relativeCancelNewAccountUrl,
            string relativeConfirmPasswordResetUrl,
            string relativeConfirmChangeEmailUrl)
        {
            this.ApplicationName = appName;
            this.EmailSignature = emailSig;
            this.RelativeLoginUrl = relativeLoginUrl;
            this.RelativeVerifyAccountUrl = relativeVerifyAccountUrl;
            this.RelativeCancelNewAccountUrl = relativeCancelNewAccountUrl;
            this.RelativeConfirmPasswordResetUrl = relativeConfirmPasswordResetUrl;
            this.RelativeConfirmChangeEmailUrl = relativeConfirmChangeEmailUrl;
        }

        protected override string GetApplicationBaseUrl()
        {
            return HttpContext.Current.GetApplicationUrl();
        }
    }
}
