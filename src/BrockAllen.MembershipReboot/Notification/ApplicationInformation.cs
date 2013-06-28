/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot
{
    public class ApplicationInformation
    {
        public string ApplicationName { get; set; }
        public string EmailSignature { get; set; }
        public string LoginUrl { get; set; }
        public string VerifyAccountUrl { get; set; }
        public string CancelNewAccountUrl { get; set; }
        public string ConfirmPasswordResetUrl { get; set; }
        //public string CancelPasswordResetUrl { get; set; }
        public string ConfirmChangeEmailUrl { get; set; }
    }
}
