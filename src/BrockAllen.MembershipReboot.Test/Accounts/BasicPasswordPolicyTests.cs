/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrockAllen.MembershipReboot.Test.Accounts
{
    [TestClass]
    public class BasicPasswordPolicyTests
    {
        [TestClass]
        public class ValidatePassword
        {
            [TestMethod]
            public void NullPassword_ReturnsFail()
            {
                var sub = new BasicPasswordPolicy();
                Assert.IsFalse(sub.ValidatePassword(null));
            }
            [TestMethod]
            public void EmptyPassword_ReturnsFail()
            {
                var sub = new BasicPasswordPolicy();
                Assert.IsFalse(sub.ValidatePassword(""));
            }

            [TestMethod]
            public void Length_ValidatesCorrectly()
            {
                var sub = new BasicPasswordPolicy();
                sub.MinLength = 3;
                Assert.IsFalse(sub.ValidatePassword("a"));
                Assert.IsFalse(sub.ValidatePassword("ab"));
                Assert.IsTrue(sub.ValidatePassword("abc"));
                Assert.IsTrue(sub.ValidatePassword("abcd"));
            }

            [TestMethod]
            public void UpperAlphas_ValidatesCorrectly()
            {
                var sub = new BasicPasswordPolicy();
                sub.UpperAlphas = 3;
                Assert.IsFalse(sub.ValidatePassword("abcd"));
                Assert.IsFalse(sub.ValidatePassword("Abcd"));
                Assert.IsFalse(sub.ValidatePassword("ABcd"));
                Assert.IsTrue(sub.ValidatePassword("ABCd"));
                Assert.IsTrue(sub.ValidatePassword("ABCD"));
            }

            [TestMethod]
            public void LowerAlphas_ValidatesCorrectly()
            {
                var sub = new BasicPasswordPolicy();
                sub.LowerAlphas = 3;
                Assert.IsFalse(sub.ValidatePassword("ABCD"));
                Assert.IsFalse(sub.ValidatePassword("aBCD"));
                Assert.IsFalse(sub.ValidatePassword("abCD"));
                Assert.IsTrue(sub.ValidatePassword("abcD"));
                Assert.IsTrue(sub.ValidatePassword("abcd"));
            }

            [TestMethod]
            public void Numerics_ValidatesCorrectly()
            {
                var sub = new BasicPasswordPolicy();
                sub.Numerics = 3;
                Assert.IsFalse(sub.ValidatePassword("abcd"));
                Assert.IsFalse(sub.ValidatePassword("abcd1"));
                Assert.IsFalse(sub.ValidatePassword("abcd12"));
                Assert.IsTrue(sub.ValidatePassword("abcd123"));
                Assert.IsTrue(sub.ValidatePassword("abcd1234"));
            }

            [TestMethod]
            public void NonAlphaNumerics_ValidatesCorrectly()
            {
                var sub = new BasicPasswordPolicy();
                sub.NonAlphaNumerics = 3;
                Assert.IsFalse(sub.ValidatePassword("abcd"));
                Assert.IsFalse(sub.ValidatePassword("abcd!"));
                Assert.IsFalse(sub.ValidatePassword("abcd!@"));
                Assert.IsTrue(sub.ValidatePassword("abcd!@#"));
                Assert.IsTrue(sub.ValidatePassword("abcd!@#$"));
            }
        }
    }
}
