﻿using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot.Test.Authentication
{
    [TestClass]
    public class SignInWithLinkedAccountTests
    {
        TestAuthenticationService subject;
        UserAccountService userAccountService;
        FakeUserAccountRepository repository;
        MembershipRebootConfiguration configuration;
        KeyNotification key;

        [TestInitialize]
        public void Init()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 1; // tests will run faster

            configuration = new MembershipRebootConfiguration
            {
                RequireAccountVerification = false
            };
            key = new KeyNotification();
            configuration.AddEventHandler(key);
            repository = new FakeUserAccountRepository();
            userAccountService = new UserAccountService(configuration, repository);

            subject = new TestAuthenticationService(userAccountService);
        }

        [TestMethod]
        public void Create_New_UserAccount_From_External_Provider()
        {
            subject.SignInWithLinkedAccount("google", "123", new[] {new Claim(ClaimTypes.Email, "test@gmail.com"),});
            Assert.AreEqual(1, repository.UserAccounts.Count);
            var addedAccount = repository.UserAccounts[0];
            Assert.AreEqual(1, addedAccount.LinkedAccounts.Count());
            var linkedAccount = addedAccount.LinkedAccounts.ElementAt(0);
            Assert.AreEqual("google", linkedAccount.ProviderName);
        }

        [TestMethod]
        public void Use_NameClaim_As_Username_For_New_UserAccount()
        {
            subject.SignInWithLinkedAccount("google", "123", new[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com"),
                new Claim(ClaimTypes.Name, "Christian")
            });
            var addedAccount = repository.UserAccounts[0];
            Assert.AreEqual("Christian", addedAccount.Username);
        }

        [TestMethod]
        public void Use_NameClaim_Stripped_Of_Invalid_Chars_For_New_UserAccount()
        {
            subject.SignInWithLinkedAccount("google", "123", new[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com"),
                new Claim(ClaimTypes.Name, " ~ Christian Crowhurst @ ")
            });
            var addedAccount = repository.UserAccounts[0];
            Assert.AreEqual("Christian Crowhurst", addedAccount.Username);
        }

        [TestMethod]
        public void Guessed_Username_Should_Strip_Invalid_Chars()
        {
            subject.SignInWithLinkedAccount("google", "123", new[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com"),
                new Claim(ClaimTypes.Name, " ~ Christian Crowhurst @ ")
            });
            var addedAccount = repository.UserAccounts[0];
            Assert.AreEqual("Christian Crowhurst", addedAccount.Username);
        }

        [TestMethod]
        [Description("For list of allowed special chars see UserAccountValidation<TAccount>")]
        public void Guessed_Username_Should_Allow_Special_Chars()
        {
            subject.SignInWithLinkedAccount("google", "123", new[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com"),
                new Claim(ClaimTypes.Name, @" ~ Christian.Forrest-Smith_OK @ ")
            });
            var addedAccount = repository.UserAccounts[0];
            Assert.AreEqual("Christian.Forrest-Smith_OK", addedAccount.Username);
        }
    }
}
