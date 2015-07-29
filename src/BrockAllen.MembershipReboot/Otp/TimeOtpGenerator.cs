using System;
using System.Linq;

namespace BrockAllen.MembershipReboot.Otp {
    public class TimeOtpGenerator : OtpGenerator
    {
        // Default values set by RFC 4226 and 6238
        private const int DEFAULT_DIGITS = 8;
        private const bool DEFAULT_USE_CHECKSUM = false;
        private const int DEFAULT_TIME_STEP = 30; // seconds 
        private static readonly DateTime T0 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Constructors

        public TimeOtpGenerator()
            : base(DEFAULT_HMAC_ALGORITHM_NAME)
        {
            // Initialize default options per RFC
            this.Digits = DEFAULT_DIGITS;
            this.UseChecksumDigit = DEFAULT_USE_CHECKSUM;
            this.TimeStep = DEFAULT_TIME_STEP;
        }

        // Properties

        public int TimeStep { get; set; }

        // Methods

        public string GenerateOneTimePassword(byte[] secret)
        {
            return this.GenerateOneTimePassword(secret, DateTime.UtcNow);
        }

        public string GenerateOneTimePassword(byte[] secret, DateTime instant)
        {
            if (secret == null || secret.Length == 0) throw new ArgumentException("Value cannot be null or empty.", "secret");
            if (this.TimeStep < 1) throw new InvalidOperationException("Cannot generate OTP when TimeStep < 1.");

            var epochTime = (long)(instant.ToUniversalTime().Subtract(T0).TotalSeconds);
            var t = (long)Math.Floor((decimal)epochTime / this.TimeStep);
            var movingFactor = BitConverter.GetBytes(t).Reverse().ToArray();
            return base.GenerateOneTimePassword(secret, movingFactor);
        }

    }
}