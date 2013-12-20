/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BrockAllen.MembershipReboot.Test.Extensions
{
    [TestClass]
    public class DisposableExtensionsTests
    {
        class FakeDisposable : IDisposable
        {
            public bool WasCalled { get; set; }
            public void Dispose()
            {
                WasCalled = true;
            }
        }

        [TestMethod]
        public void TryDispose_NullParam_ReturnsFalse()
        {
            Assert.IsFalse(DisposableExtensions.TryDispose(null));
        }

        [TestMethod]
        public void TryDispose_ValidParam_ReturnsTrueAndCallsDispose()
        {
            var fake = new FakeDisposable();
            Assert.IsTrue(DisposableExtensions.TryDispose(fake));
            Assert.IsTrue(fake.WasCalled);
        }
    }
}
