/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.Owin;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot.Owin
{
    public class OwinApplicationInformation : RelativePathApplicationInformation
    {
        IOwinContext owinContext;

        public OwinApplicationInformation(
            IDictionary<string, object> env,
            string appName,
            string emailSig,
            string relativeLoginUrl,
            string relativeConfirmChangeEmailUrl,
            string relativeCancelNewAccountUrl,
            string relativeConfirmPasswordResetUrl)
        {
            this.owinContext = new OwinContext(env);
            this.ApplicationName = appName;
            this.EmailSignature = emailSig;
            this.RelativeLoginUrl = relativeLoginUrl;
            this.RelativeCancelVerificationUrl = relativeCancelNewAccountUrl;
            this.RelativeConfirmPasswordResetUrl = relativeConfirmPasswordResetUrl;
            this.RelativeConfirmChangeEmailUrl = relativeConfirmChangeEmailUrl;
        }

        protected override string GetApplicationBaseUrl()
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
            return tmp;
        }
    }
}
