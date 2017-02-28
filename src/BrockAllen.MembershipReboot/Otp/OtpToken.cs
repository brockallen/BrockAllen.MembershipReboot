using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Otp
{
 public abstract class OtpToken {

        // Constructors

        protected OtpToken() {
            this.TokenId = Guid.NewGuid();
        }

        // Properties

        public Guid TokenId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public byte[] Secret { get; set; }

        // Methods

        public abstract string GenerateCurrentOneTimePassword();

    }
}
