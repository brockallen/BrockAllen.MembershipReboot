using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public abstract class RelativePathApplicationInformation : ApplicationInformation
    {
        public string RelativeLoginUrl { get; set; }
        public string RelativeConfirmPasswordResetUrl { get; set; }
        public string RelativeConfirmChangeEmailUrl { get; set; }
        public string RelativeCancelVerificationUrl { get; set; }

        protected abstract string GetApplicationBaseUrl();

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
                            var tmp = GetApplicationBaseUrl();
                            if (!tmp.EndsWith("/")) tmp += "/";
                            baseUrl = tmp;
                        }
                    }
                }
                return baseUrl;
            }
        }

        string CleanupPath(string path)
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
                if (String.IsNullOrWhiteSpace(base.LoginUrl))
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
                if (String.IsNullOrWhiteSpace(base.ConfirmPasswordResetUrl))
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
                if (String.IsNullOrWhiteSpace(base.ConfirmChangeEmailUrl))
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
                if (String.IsNullOrWhiteSpace(base.CancelVerificationUrl))
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
