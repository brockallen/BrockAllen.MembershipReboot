/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using Microsoft.AspNet.Http;
using HttpContext = System.Web.HttpContext;

namespace BrockAllen.MembershipReboot.WebHost
{
    public class AspNetApplicationInformation : RelativePathApplicationInformation
    {
        private readonly IHttpContextAccessor _contextAccessor;

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
            this._contextAccessor = contextAccessor;
            this.ApplicationName = appName;
            this.EmailSignature = emailSig;
            this.RelativeLoginUrl = relativeLoginUrl;
            this.RelativeConfirmChangeEmailUrl = relativeConfirmChangeEmailUrl;
            this.RelativeCancelVerificationUrl = relativeCancelVerificationUrl;
            this.RelativeConfirmPasswordResetUrl = relativeConfirmPasswordResetUrl;
        }

        protected override string GetApplicationBaseUrl()
        {
            return GetApplicationUrl(_contextAccessor.HttpContext.Request);
        }
        public static string GetApplicationUrl(HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var baseUrl = $"{request.Scheme}://{request.Host}";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            return baseUrl;
        }
    }
}
}
