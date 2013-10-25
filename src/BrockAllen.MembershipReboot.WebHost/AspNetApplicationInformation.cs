/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Web;
namespace BrockAllen.MembershipReboot.WebHost
{
    public class AspNetApplicationInformation : ApplicationInformation
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

        public string RelativeLoginUrl { get; set; }
        public string RelativeVerifyAccountUrl { get; set; }
        public string RelativeCancelNewAccountUrl { get; set; }
        public string RelativeConfirmPasswordResetUrl { get; set; }
        public string RelativeConfirmChangeEmailUrl { get; set; }

        string baseUrl;
        object urlLock = new object();
        string BaseUrl
        {
            get
            {
                if (baseUrl == null)
                {
                    lock (urlLock)
                    {
                        if (baseUrl == null)
                        {
                            // build URL
                            baseUrl = HttpContext.Current.GetApplicationUrl();
                        }
                    }
                }
                return baseUrl;
            }
        }

        public override string LoginUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.LoginUrl))
                {
                    LoginUrl = BaseUrl + RelativeLoginUrl;
                }
                return base.LoginUrl;
            }
            set
            {
                base.LoginUrl = value;
            }
        }

        public override string VerifyAccountUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.VerifyAccountUrl))
                {
                    VerifyAccountUrl = BaseUrl + RelativeVerifyAccountUrl;
                }
                return base.VerifyAccountUrl;
            }
            set
            {
                base.VerifyAccountUrl = value;
            }
        }

        public override string CancelNewAccountUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.CancelNewAccountUrl))
                {
                    CancelNewAccountUrl = BaseUrl + RelativeCancelNewAccountUrl;
                }
                return base.CancelNewAccountUrl;
            }
            set
            {
                base.CancelNewAccountUrl = value;
            }
        }

        public override string ConfirmPasswordResetUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.ConfirmPasswordResetUrl))
                {
                    ConfirmPasswordResetUrl = BaseUrl + RelativeConfirmPasswordResetUrl;
                }
                return base.ConfirmPasswordResetUrl;
            }
            set
            {
                base.ConfirmPasswordResetUrl = value;
            }
        }

        public override string ConfirmChangeEmailUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.ConfirmChangeEmailUrl))
                {
                    ConfirmChangeEmailUrl = BaseUrl + RelativeConfirmChangeEmailUrl;
                }
                return base.ConfirmChangeEmailUrl;
            }
            set
            {
                base.ConfirmChangeEmailUrl = value;
            }
        }
    }
}
