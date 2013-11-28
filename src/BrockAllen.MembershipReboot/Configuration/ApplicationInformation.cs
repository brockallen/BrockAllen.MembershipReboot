/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

namespace BrockAllen.MembershipReboot
{
    public class ApplicationInformation
    {
        public virtual string ApplicationName { get; set; }
        public virtual string EmailSignature { get; set; }

        public virtual string LoginUrl { get; set; }

        public virtual string ConfirmPasswordResetUrl { get; set; }
        public virtual string ConfirmChangeEmailUrl { get; set; }
        public virtual string CancelVerificationUrl { get; set; }
    }
}
