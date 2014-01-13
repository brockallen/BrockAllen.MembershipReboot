/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.Owin;
using Owin;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Owin
{
    public class OwinApplicationInformation : RelativePathApplicationInformation
    {
        IAppBuilder app;

        public OwinApplicationInformation(
            IAppBuilder app,
            string appName,
            string emailSig,
            string relativeLoginUrl,
            string relativeConfirmChangeEmailUrl,
            string relativeCancelNewAccountUrl,
            string relativeConfirmPasswordResetUrl)
        {
            this.app = app;
            app.Use((ctx, next) =>
            {
                if (!this.HasBaseUrl)
                {
                    this.SetBaseUrl(GetApplicationBaseUrl(ctx));
                }
                return next();
            });
            this.ApplicationName = appName;
            this.EmailSignature = emailSig;
            this.RelativeLoginUrl = relativeLoginUrl;
            this.RelativeCancelVerificationUrl = relativeCancelNewAccountUrl;
            this.RelativeConfirmPasswordResetUrl = relativeConfirmPasswordResetUrl;
            this.RelativeConfirmChangeEmailUrl = relativeConfirmChangeEmailUrl;
        }

        string GetApplicationBaseUrl(IOwinContext owinContext)
        {
            var tmp = owinContext.Request.Scheme + "://" + owinContext.Request.Host;
            if (owinContext.Request.PathBase.HasValue)
            {
                if (!owinContext.Request.PathBase.Value.StartsWith("/"))
                {
                    tmp += "/";
                }
                tmp += owinContext.Request.PathBase.Value;
            }
            else
            {
                tmp += "/";
            }
            if (!tmp.EndsWith("/")) tmp += "/"; 
            return tmp;
        }
    }
}
