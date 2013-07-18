using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.Validation
{
    [TestClass]
    public class DelegateValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_NullParam_Throws()
        {
            new DelegateValidator(null);
        }

        [TestMethod]
        public void Validate_CallsDelegate()
        {
            bool wasCalled = false;
            Func<UserAccountService, UserAccount, string, ValidationResult> func =
                (svc, ua, val) =>
                {
                    wasCalled = true;
                    return null;
                };
            var sub = new DelegateValidator(func);
            sub.Validate(null, null, null);
            Assert.IsTrue(wasCalled);
        }
    }
}
