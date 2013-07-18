using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Test.Notification
{
    [TestClass]
    public class EmailEventHandlerTests
    {
        [TestMethod]
        public void Process_FormatReturnsMessage_SendsMessage()
        {
            var msg = new Message();
            var mockMF = new Mock<IMessageFormatter>();
            var mockMD = new Mock<IMessageDelivery>();
            mockMF.Setup(x => x.Format(It.IsAny<UserAccountEvent>())).Returns(msg);

            var sub = new EmailEventHandler(mockMF.Object, mockMD.Object);

            var mockAcct = new MockUserAccount();
            var mockEvt = new Mock<UserAccountEvent>();
            mockEvt.Object.Account = mockAcct.Object;
            sub.Process(mockEvt.Object);

            mockMD.Verify(x => x.Send(msg));
        }
        [TestMethod]
        public void Process_FormatReturnsNoMessage_DoesNotSendMessage()
        {
            var mockMF = new Mock<IMessageFormatter>();
            var mockMD = new Mock<IMessageDelivery>();

            var sub = new EmailEventHandler(mockMF.Object, mockMD.Object);

            var mockAcct = new MockUserAccount();
            var mockEvt = new Mock<UserAccountEvent>();
            mockEvt.Object.Account = mockAcct.Object;
            sub.Process(mockEvt.Object);

            mockMD.Verify(x => x.Send(It.IsAny<Message>()), Times.Never());
        }
        [TestMethod]
        public void Process_NoExplicitEmailParam_UsesAccountEmail()
        {
            var msg = new Message();
            var mockMF = new Mock<IMessageFormatter>();
            var mockMD = new Mock<IMessageDelivery>();
            mockMF.Setup(x => x.Format(It.IsAny<UserAccountEvent>())).Returns(msg);

            var sub = new EmailEventHandler(mockMF.Object, mockMD.Object);

            var mockAcct = new MockUserAccount();
            mockAcct.Object.Email = "foo";
            var mockEvt = new Mock<UserAccountEvent>();
            mockEvt.Object.Account = mockAcct.Object;
            sub.Process(mockEvt.Object);

            Assert.AreEqual("foo", msg.To);
        }
        [TestMethod]
        public void Process_ExplicitEmailParam_UsesExplicitEmail()
        {
            var msg = new Message();
            var mockMF = new Mock<IMessageFormatter>();
            var mockMD = new Mock<IMessageDelivery>();
            mockMF.Setup(x => x.Format(It.IsAny<UserAccountEvent>())).Returns(msg);

            var sub = new EmailEventHandler(mockMF.Object, mockMD.Object);

            var mockAcct = new MockUserAccount();
            mockAcct.Object.Email = "foo";
            var mockEvt = new Mock<UserAccountEvent>();
            mockEvt.Object.Account = mockAcct.Object;
            sub.Process(mockEvt.Object, "bar");

            Assert.AreEqual("bar", msg.To);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_NullFormatter_Throws()
        {
            var mockMF = new Mock<IMessageFormatter>();
            var mockMD = new Mock<IMessageDelivery>();
            var sub = new EmailEventHandler(null, mockMD.Object);
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ctor_NullDelivery_Throws()
        {
            var mockMF = new Mock<IMessageFormatter>();
            var mockMD = new Mock<IMessageDelivery>();
            var sub = new EmailEventHandler(mockMF.Object, null);
        }
    }
}
