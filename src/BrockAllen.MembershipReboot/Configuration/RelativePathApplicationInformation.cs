/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class RelativePathApplicationInformation : ApplicationInformation
    {
        public RelativePathApplicationInformation()
        {
        }

        public RelativePathApplicationInformation(string baseUrl) 
        {
            this.baseUrl = baseUrl;
        }

        public string RelativeLoginUrl { get; set; }
        public string RelativeConfirmPasswordResetUrl { get; set; }
        public string RelativeConfirmChangeEmailUrl { get; set; }
        public string RelativeCancelVerificationUrl { get; set; }

      
        /// <summary>
        /// Specifies whether Urls exposed by this class are cached or not.
        /// </summary>
        /// <remarks>
        /// Urls are cached by default once initially resolved. For some scenarios this might not be desirable. For example, a multi-tenant app which
        /// identifies tenants by subdomain name: there will only be one single (logical) instance of the app, but it will be accessed through different
        /// Urls. In this case the Urls shouldn't be cached as they will be tenant-specific and this flag can be used to turn caching off.
        /// </remarks>
        public bool IsUrlCachingDisabled { get; set; }

        protected virtual string GetApplicationBaseUrl()
        {
            throw new NotImplementedException("Either set baseUrl as ctor param or derive and override GetApplicationBaseUrl");
        }

        public bool HasBaseUrl
        {
            get { return baseUrl != null && IsUrlCachingDisabled == false; }
        }

        protected void SetBaseUrl(string url)
        {
            this.baseUrl = url;
        }

        string baseUrl;
        object urlLock = new object();
        protected string BaseUrl
        {
            get
            {
                if (!HasBaseUrl || IsUrlCachingDisabled)
                {
                    lock (urlLock)
                    {
                        if (!HasBaseUrl || IsUrlCachingDisabled)
                        {
                            // build URL
                            var tmp = GetApplicationBaseUrl();
                            if (!tmp.EndsWith("/")) tmp += "/";
                            baseUrl = tmp;
                        }
                    }
                }
                return baseUrl;
            }
        }

        protected string CleanupPath(string path)
        {
            if (path.StartsWith("~/"))
            {
                return path.Substring(2);
            }
            if (path.StartsWith("/"))
            {
                return path.Substring(1);
            }
            return path;
        }

        public override string LoginUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.LoginUrl) || IsUrlCachingDisabled)
                {
                    LoginUrl = BaseUrl + CleanupPath(RelativeLoginUrl);
                }
                return base.LoginUrl;
            }
            set
            {
                base.LoginUrl = value;
            }
        }

        public override string ConfirmPasswordResetUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.ConfirmPasswordResetUrl) || IsUrlCachingDisabled)
                {
                    ConfirmPasswordResetUrl = BaseUrl + CleanupPath(RelativeConfirmPasswordResetUrl);
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
                if (String.IsNullOrWhiteSpace(base.ConfirmChangeEmailUrl) || IsUrlCachingDisabled)
                {
                    ConfirmChangeEmailUrl = BaseUrl + CleanupPath(RelativeConfirmChangeEmailUrl);
                }
                return base.ConfirmChangeEmailUrl;
            }
            set
            {
                base.ConfirmChangeEmailUrl = value;
            }
        }

        public override string CancelVerificationUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(base.CancelVerificationUrl) || IsUrlCachingDisabled)
                {
                    CancelVerificationUrl = BaseUrl + CleanupPath(RelativeCancelVerificationUrl);
                }
                return base.CancelVerificationUrl;
            }
            set
            {
                base.CancelVerificationUrl = value;
            }
        }
    }
}
