using System;

namespace BrockAllen.MembershipReboot.Otp {
    public class TimeOtpToken : OtpToken
    {

        // Constructors

        public TimeOtpToken()
            : base()
        {
            this.Generator = new TimeOtpGenerator();
        }

        // Properties

        public TimeOtpGenerator Generator { get; set; }

        // Methods

        public override string GenerateCurrentOneTimePassword()
        {
            if (this.Generator == null) throw new InvalidOperationException("Cannot generate OTP when Generator property is null.");
            if (this.Secret == null || this.Secret.Length == 0) throw new InvalidOperationException("Cannot generate OTP when Secret property is null or empty.");

            return this.Generator.GenerateOneTimePassword(this.Secret);
        }

    }
}