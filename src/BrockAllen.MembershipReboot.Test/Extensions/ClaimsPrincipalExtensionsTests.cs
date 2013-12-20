/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot.Test.Extensions
{
    [TestClass]
    public class ClaimsPrincipalExtensionsTests
    {
        [TestMethod]
        public void HasClaim_NullPrincipal_ReturnsFalse()
        {
            ClaimsPrincipal cp = null;
            Assert.IsFalse(cp.HasClaim("type"));
        }
        [TestMethod]
        public void HasClaim_DoesNotHaveClaim_ReturnsFalse()
        {
            var ci = new ClaimsIdentity(new Claim[]
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
            });
            var cp = new ClaimsPrincipal(ci);
            Assert.IsFalse(cp.HasClaim("type3"));
        }
        [TestMethod]
        public void HasClaim_DoesHaveClaim_ReturnsTrue()
        {
            var ci = new ClaimsIdentity(new Claim[]
            {
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
            });
            var cp = new ClaimsPrincipal(ci);
            Assert.IsTrue(cp.HasClaim("type1"));
        }
    }
}
