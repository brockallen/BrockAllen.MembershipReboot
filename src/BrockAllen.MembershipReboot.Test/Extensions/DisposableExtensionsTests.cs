/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BrockAllen.MembershipReboot.Test.Extensions
{
    [TestClass]
    public class DisposableExtensionsTests
    {
        [TestMethod]
        public void TryDispose_NullParam_ReturnsFalse()
        {
            Assert.IsFalse(DisposableExtensions.TryDispose(null));
        }

        [TestMethod]
        public void TryDispose_ValidParam_ReturnsTrueAndCallsDispose()
        {
            var mockDisposable = new Mock<IDisposable>();
            Assert.IsTrue(DisposableExtensions.TryDispose(mockDisposable.Object));
            mockDisposable.Verify(x => x.Dispose());
        }
    }
}
