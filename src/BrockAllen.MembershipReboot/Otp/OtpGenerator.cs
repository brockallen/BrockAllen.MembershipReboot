using System;
using System.Security.Cryptography;

namespace BrockAllen.MembershipReboot.Otp {
    public abstract class OtpGenerator
    {
        protected const string DEFAULT_HMAC_ALGORITHM_NAME = "HMACSHA1";

        // Constructors
        protected OtpGenerator(string hmacAlgorithmName)
        {
            if (hmacAlgorithmName == null) throw new ArgumentNullException("hmacAlgorithmName");
            if (string.IsNullOrWhiteSpace(hmacAlgorithmName)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "hmacAlgorithmName");

            this.HmacAlgorithmName = hmacAlgorithmName;
        }

        // Properties

        public int Digits { get; set; }

        public bool UseChecksumDigit { get; set; }

        public string HmacAlgorithmName { get; set; }

        // Methods

        protected string GenerateOneTimePassword(byte[] secret, byte[] movingFactor)
        {
            // validate arguments
            if (movingFactor == null || movingFactor.Length == 0) throw new ArgumentException("Value cannot be null or empty.", "movingFactor");
            if (secret == null || secret.Length == 0) throw new ArgumentException("Value cannot be null or empty.", "secret");
            if (string.IsNullOrWhiteSpace(this.HmacAlgorithmName)) throw new InvalidOperationException("Cannot generate OTP when HmacAlgorithmName is null or empty.");

            // Compute HMAC
            using (var hmacGenerator = HMAC.Create(this.HmacAlgorithmName))
            {
                if (hmacGenerator == null) throw new InvalidOperationException(string.Format("Cannot create HMAC for algorithm '{0}'.", this.HmacAlgorithmName));
                hmacGenerator.Key = secret;
                var mac = hmacGenerator.ComputeHash(movingFactor);

                // Get dynamic offset from low 4-bits of last byte of mac
                var offset = mac[mac.Length - 1] & 0xf;

                // Get 31-bits of the mac based on offset
                var otp = (mac[offset] << 24) | (mac[offset + 1] << 16) | (mac[offset + 2] << 8) | mac[offset + 3];
                otp = otp & 0x7FFFFFFF;
                otp = otp % (int)Math.Pow(10, this.Digits);
                var otpString = otp.ToString(new string('0', this.Digits));
                if (this.UseChecksumDigit) otpString = this.AddChecksumDigit(otpString);
                return otpString;
            }
        }

        public virtual string AddChecksumDigit(string data)
        {
            int sum = 0;
            bool odd = true;
            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (odd == true)
                {
                    int tSum = Convert.ToInt32(data[i].ToString()) * 2;
                    if (tSum >= 10)
                    {
                        string tData = tSum.ToString();
                        tSum = Convert.ToInt32(tData[0].ToString()) + Convert.ToInt32(tData[1].ToString());
                    }
                    sum += tSum;
                }
                else
                    sum += Convert.ToInt32(data[i].ToString());
                odd = !odd;
            }
            var checkNumber = ((((sum / 10) + 1) * 10) - sum) % 10;
            return string.Concat(data, checkNumber);
        }

    }
}