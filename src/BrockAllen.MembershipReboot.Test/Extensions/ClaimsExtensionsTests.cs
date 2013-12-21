/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot.Test.Extensions
{
    [TestClass]
    public class ClaimsExtensionsTests
    {
        [TestMethod]
        public void GetValue_NullClaims_ReturnsNull()
        {
            IEnumerable<Claim> claims = null;
            Assert.IsNull(claims.GetValue("type"));
        }
        [TestMethod]
        public void GetValue_NoMatches_ReturnsNull()
        {
            IEnumerable<Claim> claims = new Claim[]{
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
            };
            Assert.IsNull(claims.GetValue("type3"));
        }
        [TestMethod]
        public void GetValue_OneMatches_ReturnsValue()
        {
            IEnumerable<Claim> claims = new Claim[]{
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
            };
            Assert.AreEqual("value1", claims.GetValue("type1"));
        }
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetValue_ManyMatches_Throws()
        {
            IEnumerable<Claim> claims = new Claim[]{
                new Claim("type1", "value1"),
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
            };
            claims.GetValue("type1");
        }

        [TestMethod]
        public void GetValues_NullClaims_ReturnsEmptyResult()
        {
            IEnumerable<Claim> claims = null;
            var result = claims.GetValues("type");
            Assert.AreEqual(0, result.ToArray().Count());
        }
        [TestMethod]
        public void GetValues_NoMatches_ReturnsEmptyResult()
        {
            IEnumerable<Claim> claims = new Claim[]{
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
            };
            var result = claims.GetValues("type3");
            Assert.AreEqual(0, result.ToArray().Count());
        }
        [TestMethod]
        public void GetValues_OneMatch_ReturnsCorrectValue()
        {
            IEnumerable<Claim> claims = new Claim[]{
                new Claim("type1", "value1"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3"),
            };
            var result = claims.GetValues("type1");
            Assert.AreEqual(1, result.ToArray().Count());
            Assert.AreEqual("value1", result.First());
        }
        [TestMethod]
        public void GetValues_ManyMatches_ReturnsCorrectValues()
        {
            IEnumerable<Claim> claims = new Claim[]{
                new Claim("type1", "value11"),
                new Claim("type1", "value12"),
                new Claim("type2", "value2"),
                new Claim("type3", "value3"),
            };
            var result = claims.GetValues("type1");
            Assert.AreEqual(2, result.ToArray().Count());
            CollectionAssert.AreEquivalent(new string[] { "value11", "value12" }, result.ToArray());
        }
    }
}
