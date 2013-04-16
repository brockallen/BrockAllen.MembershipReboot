using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public enum VerificationKeyPurpose
    {
        VerifyAccount = 1,
        ChangePassword = 2,
        ChangeEmail = 3
    }
}
