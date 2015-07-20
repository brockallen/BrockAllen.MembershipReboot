﻿using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using BrockAllen.MembershipReboot.Relational;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BrockAllen.MembershipReboot.Test.Authentication
{
    [TestClass]
    public class SignInTests
    {
        TestAuthenticationService subject;
        UserAccountService userAccountService;
        FakeUserAccountRepository repository;

        [TestInitialize]
        public void Init()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 1; // tests will run faster

            var configuration = new MembershipRebootConfiguration
            {
                RequireAccountVerification = false,
                PasswordResetFrequency = 1 // every day
            };
            configuration.AddEventHandler(new KeyNotification());
            configuration.AddCommandHandler(new TestMapClaimsFromAccount());
            repository = new FakeUserAccountRepository();
            userAccountService = new UserAccountService(configuration, repository);

            subject = new TestAuthenticationService(userAccountService);
        }

        [TestMethod]
        public void SignedIn_ClaimsPrinicipal_Claims()
        {
            // given
            UserAccount acc = userAccountService.CreateAccount("test", "abcdefg123", "test@gmail.com");
            acc.MobilePhoneNumber = "07506777777";

            // when
            subject.SignIn(acc, "Bearer");

            // then
            AssertHasBasicClaims(subject.CurrentPrincipal);
            Assert.IsTrue(subject.CurrentPrincipal.HasClaim(ClaimTypes.Email));
            Assert.IsTrue(subject.CurrentPrincipal.HasClaim(ClaimTypes.MobilePhone));
            Assert.IsTrue(subject.CurrentPrincipal.HasClaim("TestClaim"));
        }


        [TestMethod]
        public void SignedIn_RequiresTwoFactorAuthToSignIn_Claims()
        {
            // given... 

            UserAccount acc = userAccountService.CreateAccount("test", "abcdefg123", "test@gmail.com");
            acc.MobilePhoneNumber = "07506777777";
            acc.AccountTwoFactorAuthMode = TwoFactorAuthMode.Mobile;
            // fake a request for two-factor auth
            acc.CurrentTwoFactorAuthStatus = TwoFactorAuthMode.Mobile;

            // when
            subject.SignIn(acc, "Bearer");

            // then
            AssertHasBasicClaims(subject.CurrentPrincipal);
            Assert.AreEqual(PartialAuthReason.PendingTwoFactorAuth, subject.CurrentPrincipal.GetPartialAuthReason());
            Assert.AreEqual(TwoFactorAuthMode.Mobile.ToString(), 
                subject.CurrentPrincipal.Claims.GetValue(MembershipRebootConstants.ClaimTypes.PendingTwoFactorAuth));
            Assert.IsFalse(subject.CurrentPrincipal.HasClaim("TestClaim"));

        }

        [TestMethod]
        public void SignedIn_RequiresPasswordReset_Claims()
        {
            // given... 
            UserAccount acc = userAccountService.CreateAccount("test", "abcdefg123", "test@gmail.com");
            acc.RequiresPasswordReset = true;

            // when
            subject.SignIn(acc, "Bearer");

            // then
            AssertHasBasicClaims(subject.CurrentPrincipal);
            Assert.AreEqual(PartialAuthReason.PasswordResetRequired, subject.CurrentPrincipal.GetPartialAuthReason());
            Assert.IsFalse(subject.CurrentPrincipal.HasClaim("TestClaim"));
        }

        [TestMethod]
        public void SignedIn_PasswordExpired_Claims()
        {
            // given... 
            UserAccount acc = userAccountService.CreateAccount("test", "abcdefg123", "test@gmail.com");
            acc.PasswordChanged = DateTime.Now.AddMonths(-1);

            // when
            subject.SignIn(acc, "Bearer");

            // then
            AssertHasBasicClaims(subject.CurrentPrincipal);
            Assert.AreEqual(PartialAuthReason.PasswordExpired, subject.CurrentPrincipal.GetPartialAuthReason());
            Assert.IsFalse(subject.CurrentPrincipal.HasClaim("TestClaim"));
        }

        private void AssertHasBasicClaims(ClaimsPrincipal signingInUser)
        {
            Assert.AreEqual("Bearer", signingInUser.Claims.GetValue(ClaimTypes.AuthenticationMethod));
            Assert.IsTrue(signingInUser.HasClaim(ClaimTypes.AuthenticationInstant));
            Assert.IsTrue(signingInUser.HasClaim(ClaimTypes.NameIdentifier));
            Assert.IsTrue(signingInUser.HasClaim(ClaimTypes.Name));
            Assert.IsTrue(signingInUser.HasClaim(MembershipRebootConstants.ClaimTypes.Tenant));
        }

        public class TestMapClaimsFromAccount : ICommandHandler<MapClaimsFromAccount<UserAccount>>
        {
            public void Handle(MapClaimsFromAccount<UserAccount> cmd)
            {
                cmd.MappedClaims = new[] { new Claim("TestClaim", "TestClaim") };
            }
        }
    }
}