/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace BrockAllen.MembershipReboot.Test.Crypto
{
    [TestClass]
    public class CryptoHelperTests
    {
        public class TestCrypto : DefaultCrypto
        {
            public override int GetCurrentYear()
            {
                return 2012;
            }
        }

        TestCrypto crypto = new TestCrypto();
        MembershipRebootConfiguration config = new MembershipRebootConfiguration();

        const int IterationsForCurrentYear = 64000;

        [TestMethod]
        public void HashPassword_CountStoredInHashedPassword()
        {
            {
                var result = crypto.HashPassword("pass", SecuritySettings.Instance.PasswordHashingIterationCount);
                StringAssert.StartsWith(result, crypto.EncodeIterations(IterationsForCurrentYear) + DefaultCrypto.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 5000;
                var result = crypto.HashPassword("pass", SecuritySettings.Instance.PasswordHashingIterationCount);
                StringAssert.StartsWith(result, crypto.EncodeIterations(5000) + DefaultCrypto.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 10000;
                var result = crypto.HashPassword("pass", SecuritySettings.Instance.PasswordHashingIterationCount);
                StringAssert.StartsWith(result, crypto.EncodeIterations(10000) + DefaultCrypto.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 50;
                var result = crypto.HashPassword("pass", SecuritySettings.Instance.PasswordHashingIterationCount);
                StringAssert.StartsWith(result, crypto.EncodeIterations(50) + DefaultCrypto.PasswordHashingIterationCountSeparator);
            }
        }

        [TestMethod]
        public void NegativeCount_UsesCurrentYearPrefix()
        {
            var result = crypto.HashPassword("pass", -1);
            StringAssert.StartsWith(result, crypto.EncodeIterations(IterationsForCurrentYear) + DefaultCrypto.PasswordHashingIterationCountSeparator);
        }

        [TestMethod]
        public void ZeroCount_UsesCurrentYearPrefix()
        {
            var result = crypto.HashPassword("pass", 0);
            StringAssert.StartsWith(result, crypto.EncodeIterations(IterationsForCurrentYear) + DefaultCrypto.PasswordHashingIterationCountSeparator);
        }

        [TestMethod]
        public void HashedPassword_Verifies()
        {
            var hash = crypto.HashPassword("pass", 5000);
            Assert.IsTrue(crypto.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void IncorrectPassword_DoesNotVerify()
        {
            var hash = crypto.HashPassword("pass1", 5000);
            Assert.IsFalse(crypto.VerifyHashedPassword(hash, "pass2"));
        }
        
        [TestMethod]
        public void PasswordHashingIterationCountChangedAfterHash_StillVerifies()
        {
            var hash = crypto.HashPassword("pass", 5000);
            Assert.IsTrue(crypto.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void PasswordWithoutPrefix_StillValidatesWithDefault()
        {
            var hash = crypto.HashPassword("pass", config.PasswordHashingIterationCount);
            Assert.IsTrue(crypto.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void IncorrectPrefix_DoesNotVerify()
        {
            {
                var hash = crypto.HashPassword("pass", config.PasswordHashingIterationCount);
                Assert.IsFalse(crypto.VerifyHashedPassword(crypto.EncodeIterations(5000) + "." + hash, "pass"));
            }
            {
                var hash = crypto.HashPassword("pass", config.PasswordHashingIterationCount);
                Assert.IsFalse(crypto.VerifyHashedPassword(crypto.EncodeIterations(5000) + ".5." + hash, "pass"));
            }
            {
                var hash = crypto.HashPassword("pass", config.PasswordHashingIterationCount);
                Assert.IsFalse(crypto.VerifyHashedPassword("hello." + hash, "pass"));
            }
            {
                var hash = crypto.HashPassword("pass", config.PasswordHashingIterationCount);
                Assert.IsFalse(crypto.VerifyHashedPassword("-1." + hash, "pass"));
            }
            {
                var hash = crypto.HashPassword("pass", 10000);
                hash = hash.Replace(crypto.EncodeIterations(10000), crypto.EncodeIterations(5000));
                Assert.IsFalse(crypto.VerifyHashedPassword(hash, "pass"));
            }
        }

        [TestMethod]
        public void GetIterationsFromYear_CalculatesCorrectValues()
        {
            Assert.AreEqual(1000, crypto.GetIterationsFromYear(-1));
            Assert.AreEqual(1000, crypto.GetIterationsFromYear(1999));
            Assert.AreEqual(1000, crypto.GetIterationsFromYear(2000));
            Assert.AreEqual(1000, crypto.GetIterationsFromYear(2001));

            Assert.AreEqual(2000, crypto.GetIterationsFromYear(2002));
            Assert.AreEqual(2000, crypto.GetIterationsFromYear(2003));

            Assert.AreEqual(4000, crypto.GetIterationsFromYear(2004));

            Assert.AreEqual(8000, crypto.GetIterationsFromYear(2006));

            Assert.AreEqual(16000, crypto.GetIterationsFromYear(2008));

            Assert.AreEqual(32000, crypto.GetIterationsFromYear(2010));

            Assert.AreEqual(64000, crypto.GetIterationsFromYear(2012));

            Assert.AreEqual(2097152000, crypto.GetIterationsFromYear(2042));

            Assert.AreEqual(Int32.MaxValue, crypto.GetIterationsFromYear(2044));
            Assert.AreEqual(Int32.MaxValue, crypto.GetIterationsFromYear(2045));
            Assert.AreEqual(Int32.MaxValue, crypto.GetIterationsFromYear(2046));
        }
    }
}
