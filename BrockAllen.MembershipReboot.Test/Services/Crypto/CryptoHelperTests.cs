using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrockAllen.MembershipReboot.Test.Services.Crypto
{
    [TestClass]
    public class CryptoHelperTests
    {
        [TestInitialize]
        public void Init()
        {
            SecuritySettings.Instance = new SecuritySettings();
        }

        [TestMethod]
        public void HashPassword_CountStoredInHashedPassword()
        {
            {
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, SecuritySettings.Instance.PasswordHashingIterationCount.ToString() + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 5000;
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, "5000" + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 10000;
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, "10000" + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 50;
                var result = CryptoHelper.HashPassword("pass");
                StringAssert.StartsWith(result, "50" + CryptoHelper.PasswordHashingIterationCountSeparator);
            }
        }

        [TestMethod]
        public void NegativeCount_UsesNoPrefix()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = -1;
            var result = CryptoHelper.HashPassword("pass");
            Assert.IsFalse(result.Contains(CryptoHelper.PasswordHashingIterationCountSeparator));
        }

        [TestMethod]
        public void ZeroCount_UsesNoPrefix()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 0;
            var result = CryptoHelper.HashPassword("pass");
            Assert.IsFalse(result.Contains(CryptoHelper.PasswordHashingIterationCountSeparator));
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
            var hash = System.Web.Helpers.Crypto.HashPassword("pass");
            Assert.IsTrue(CryptoHelper.VerifyHashedPassword(hash, "pass"));
        }

        [TestMethod]
        public void IncorrectPrefix_DoesNotVerify()
        {
            {
                var hash = System.Web.Helpers.Crypto.HashPassword("pass");
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword("5000." + hash, "pass"));
            }
            {
                SecuritySettings.Instance.PasswordHashingIterationCount = 10000;
                var hash = CryptoHelper.HashPassword("pass");
                hash = hash.Replace("10000", "5000");
                Assert.IsFalse(CryptoHelper.VerifyHashedPassword(hash, "pass"));
            }
        }
    }
}
