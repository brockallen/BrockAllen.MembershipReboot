﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace BrockAllen.MembershipReboot.Test.Accounts
{
    [TestClass]
    public class SecuritySettingsTests
    {
        [TestMethod]
        public void TestDefaults()
        {
            var settings = new SecuritySettings();
            Assert.AreEqual(false, settings.MultiTenant);
            Assert.AreEqual("default", settings.DefaultTenant);
            Assert.AreEqual(false, settings.EmailIsUsername);
            Assert.AreEqual(true, settings.EmailIsUnique);
            Assert.AreEqual(false, settings.UsernamesUniqueAcrossTenants);
            Assert.AreEqual(true, settings.RequireAccountVerification);
            Assert.AreEqual(true, settings.AllowLoginAfterAccountCreation);
            Assert.AreEqual(5, settings.AccountLockoutFailedLoginAttempts);
            Assert.AreEqual(TimeSpan.FromMinutes(5), settings.AccountLockoutDuration);
            Assert.AreEqual(true, settings.AllowAccountDeletion);
            Assert.AreEqual(0, settings.PasswordHashingIterationCount);
            Assert.AreEqual(0, settings.PasswordResetFrequency);
            Assert.AreEqual(TimeSpan.FromMinutes(20), settings.VerificationKeyLifetime);
            Assert.AreEqual(true, settings.CertificateIsUnique);
            Assert.AreEqual(true, settings.MobilePhoneIsUnique);
            Assert.AreEqual(false, settings.RequirePasswordResetAfterAccountCreation);
        }
    }
}
