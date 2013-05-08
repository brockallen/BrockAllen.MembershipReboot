using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public enum LinkedAccountSignInStatus
    {
        Success,
        Success_NewAccount,
        Failure_NewAccountNoEmailInClaims,
        Failure_AccountNotVerified,
        Failure_LoginNotAllowed
    }

    public static class LinkedAccountSignInResultExtensions
    {
        public static bool IsSuccess(this LinkedAccountSignInStatus result)
        {
            return
                result == LinkedAccountSignInStatus.Success ||
                result == LinkedAccountSignInStatus.Success_NewAccount;
        }
    }
}
