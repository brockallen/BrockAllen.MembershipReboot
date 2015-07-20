﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.Authentication
{
    class TestAuthenticationService : AuthenticationService
    {
        public TestAuthenticationService(UserAccountService svc)
            : base(svc)
        {
        }

        public ClaimsPrincipal CurrentPrincipal { get; set; }
        protected override System.Security.Claims.ClaimsPrincipal GetCurrentPrincipal()
        {
            return CurrentPrincipal;
        }

        protected override void IssueToken(System.Security.Claims.ClaimsPrincipal principal, TimeSpan? tokenLifetime = null, bool? persistentCookie = null)
        {
            CurrentPrincipal = principal;
        }

        protected override void RevokeToken()
        {
        }
    }
}
