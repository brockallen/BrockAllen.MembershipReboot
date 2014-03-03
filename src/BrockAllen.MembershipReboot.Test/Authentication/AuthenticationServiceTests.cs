using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace BrockAllen.MembershipReboot.Test.Authentication
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        TestAuthenticationService subject;
        UserAccountService userAccountService;
        FakeUserAccountRepository repository;
        MembershipRebootConfiguration configuration;
        KeyNotification key;

        int oldIterations;

        [TestInitialize]
        public void Init()
        {
            oldIterations = SecuritySettings.Instance.PasswordHashingIterationCount;
            SecuritySettings.Instance.PasswordHashingIterationCount = 1; // tests will run faster

            configuration = new MembershipRebootConfiguration();
            key = new KeyNotification();
            configuration.AddEventHandler(key);
            repository = new FakeUserAccountRepository();
            userAccountService = new UserAccountService(configuration, repository);

            subject = new TestAuthenticationService(userAccountService);
        }

        [TestMethod]
        public void SignInWithLinkedAccount_CreatesNewAccountFromExternalProvider()
        {
            subject.SignInWithLinkedAccount("google", "123", new Claim[] { });
        }
    }
}
