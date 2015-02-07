/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BrockAllen.MembershipReboot.Test.Accounts
{
    [TestClass]
    public class MembershipRebootConfigurationTests
    {
        [TestMethod]
        public void EmailNotUnique_EmailIsUsername_FailsValidation()
        {
            var subject = new MembershipRebootConfiguration();
            subject.EmailIsUnique = false;
            subject.EmailIsUsername = true;
            try
            {
                subject.Validate();
                Assert.Fail("Expected Exception");
            }
            catch (InvalidOperationException) { }
        }
    }
}
