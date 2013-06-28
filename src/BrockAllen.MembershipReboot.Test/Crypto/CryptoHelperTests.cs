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
        [TestInitialize]
        public void Init()
        {
            SecuritySettings.Instance = new SecuritySettings();
            CryptoHelper.GetCurrentYear = () => 2012;
        }

        const int IterationsForCurrentYear = 64000;

        [TestMethod]
        public void HashPassword_CountStoredInHashedPassword()
        {
            {
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, CryptoHelper.EncodeIterations(IterationsForCurrentYear) + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 5000;
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, CryptoHelper.EncodeIterations(5000) + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 10000;
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, CryptoHelper.EncodeIterations(10000) + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 50;
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, CryptoHelper.EncodeIterations(50) + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
        }

        [TestMethod]
        public void NegativeCount_UsesCurrentYearPrefix()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = -1;
            var result = CryptoHelper.HashPassword("pass");
            StringAssert.StartsWith(result, CryptoHelper.EncodeIterations(IterationsForCurrentYear) + CryptoHelper.PasswordHashingIterationCountSeparator);
        }

        [TestMethod]
        public void ZeroCount_UsesCurrentYearPrefix()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 0;
            var result = CryptoHelper.HashPassword("pass");
            StringAssert.StartsWith(result, CryptoHelper.EncodeIterations(IterationsForCurrentYear) + CryptoHelper.PasswordHashingIterationCountSeparator);
        }

        [TestMethod]
        public void HashedPassword_Verifies()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 5000;

            var hash = CryptoHelper.HashPassword("pass");
            Assert.IsTrue(CryptoHelper.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void IncorrectPassword_DoesNotVerify()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 5000;
            var hash = CryptoHelper.HashPassword("pass1");
            Assert.IsFalse(CryptoHelper.VerifyHashedPassword(hash, "pass2"));
        }
        
        [TestMethod]
        public void PasswordHashingIterationCountChangedAfterHash_StillVerifies()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 5000;
            var hash = CryptoHelper.HashPassword("pass");
            SecuritySettings.Instance.PasswordHashingIterationCount = 1000;
            Assert.IsTrue(CryptoHelper.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void PasswordWithoutPrefix_StillValidatesWithDefault()
        {
            var hash = BrockAllen.MembershipReboot.Helpers.Crypto.HashPassword("pass");
            Assert.IsTrue(CryptoHelper.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void IncorrectPrefix_DoesNotVerify()
        {
            {
                var hash = BrockAllen.MembershipReboot.Helpers.Crypto.HashPassword("pass");
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword(CryptoHelper.EncodeIterations(5000) + "." + hash, "pass"));
            }
            {
                var hash = BrockAllen.MembershipReboot.Helpers.Crypto.HashPassword("pass");
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword(CryptoHelper.EncodeIterations(5000) + ".5." + hash, "pass"));
            }
            {
                var hash = BrockAllen.MembershipReboot.Helpers.Crypto.HashPassword("pass");
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword("hello." + hash, "pass"));
            }
            {
                var hash = BrockAllen.MembershipReboot.Helpers.Crypto.HashPassword("pass");
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword("-1." + hash, "pass"));
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 10000;
                var hash = CryptoHelper.HashPassword("pass");
                hash = hash.Replace(CryptoHelper.EncodeIterations(10000), CryptoHelper.EncodeIterations(5000));
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword(hash, "pass"));
            }
        }

        [TestMethod]
        public void GetIterationsFromYear_CalculatesCorrectValues()
        {
            Assert.AreEqual(1000, CryptoHelper.GetIterationsFromYear(-1));
            Assert.AreEqual(1000, CryptoHelper.GetIterationsFromYear(1999));
            Assert.AreEqual(1000, CryptoHelper.GetIterationsFromYear(2000));
            Assert.AreEqual(1000, CryptoHelper.GetIterationsFromYear(2001));
            
            Assert.AreEqual(2000, CryptoHelper.GetIterationsFromYear(2002));
            Assert.AreEqual(2000, CryptoHelper.GetIterationsFromYear(2003));
            
            Assert.AreEqual(4000, CryptoHelper.GetIterationsFromYear(2004));
            
            Assert.AreEqual(8000, CryptoHelper.GetIterationsFromYear(2006));
            
            Assert.AreEqual(16000, CryptoHelper.GetIterationsFromYear(2008));

            Assert.AreEqual(32000, CryptoHelper.GetIterationsFromYear(2010));

            Assert.AreEqual(64000, CryptoHelper.GetIterationsFromYear(2012));

            Assert.AreEqual(2097152000, CryptoHelper.GetIterationsFromYear(2042));
            
            Assert.AreEqual(Int32.MaxValue, CryptoHelper.GetIterationsFromYear(2044));
            Assert.AreEqual(Int32.MaxValue, CryptoHelper.GetIterationsFromYear(2045));
            Assert.AreEqual(Int32.MaxValue, CryptoHelper.GetIterationsFromYear(2046));
        }
    }
}
