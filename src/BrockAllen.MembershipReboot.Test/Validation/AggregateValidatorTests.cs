using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.Validation
{
    [TestClass]
    public class AggregateValidatorTests
    {
        [TestMethod]
        public void Validate_NullService_Throws()
        {
            var sub = new AggregateValidator();
            try
            {
                var result = sub.Validate(null, new MockUserAccount().Object, "test");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void Validate_NullAccount_Throws()
        {
            var sub = new AggregateValidator();
            try
            {
                var result = sub.Validate(new MockUserAccountService().Object, null, "test");
                Assert.Fail();
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public void Validate_GetsErrorFromItems()
        {
            var mockVal = new Mock<IValidator>();
            var error = new ValidationResult("Error");
            mockVal.Setup(x => x.Validate(It.IsAny<UserAccountService>(), It.IsAny<UserAccount>(), It.IsAny<string>())).Returns(error);

            var sub = new AggregateValidator();
            sub.Add(mockVal.Object);

            var result = sub.Validate(new MockUserAccountService().Object, new MockUserAccount().Object, "test");
            Assert.AreSame(error, result);
        }

        [TestMethod]
        public void Validate_AllCallsSucceed_CallsAllValidators()
        {
            var mockVal1 = new Mock<IValidator>();
            var mockVal2 = new Mock<IValidator>();

            var sub = new AggregateValidator();
            sub.Add(mockVal1.Object);
            sub.Add(mockVal2.Object);

            var svc = new MockUserAccountService();
            var ua = new MockUserAccount();
            var val = "test";
            var result = sub.Validate(svc.Object, ua.Object, val);
            
            mockVal1.Verify(x=>x.Validate(svc.Object, ua.Object, val));
            mockVal2.Verify(x=>x.Validate(svc.Object, ua.Object, val));
        }

        [TestMethod]
        public void Validate_AllCallsSucceed_ReturnsNull()
        {
            var mockVal1 = new Mock<IValidator>();
            var mockVal2 = new Mock<IValidator>();

            var sub = new AggregateValidator();
            sub.Add(mockVal1.Object);
            sub.Add(mockVal2.Object);

            var result = sub.Validate(new MockUserAccountService().Object, new MockUserAccount().Object, "test");
            Assert.IsNull(result);
        }
    }
}
