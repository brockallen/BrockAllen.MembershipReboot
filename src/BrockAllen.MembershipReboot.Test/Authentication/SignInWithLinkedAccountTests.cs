using System;
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
        CaptureLatestEvent<AccountCreatedEvent<UserAccount>, UserAccount> accountCreatedEvent;


        public class TestMapClaimsToAccountHandler : ICommandHandler<MapClaimsToAccount<UserAccount>>
        {
            public void Handle(MapClaimsToAccount<UserAccount> cmd)
            {
                var givenName = cmd.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
                if (givenName != null)
                {
                    ((MyUserAccount) cmd.Account).FirstName = givenName.Value;
                }
            }
        }

        [TestInitialize]
        public void Init()
        {
            SecuritySettings.Instance.PasswordHashingIterationCount = 1; // tests will run faster

            configuration = new MembershipRebootConfiguration
            {
                RequireAccountVerification = false
            };
            accountCreatedEvent = CaptureLatestEvent.For<AccountCreatedEvent<UserAccount>>();
            configuration.AddEventHandler(accountCreatedEvent);
            configuration.AddCommandHandler(new TestMapClaimsToAccountHandler());
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

        
        [TestMethod]
        public void Should_Use_Registered_MapClaimsToAccount_Handler_To_Set_Properties()
        {
            subject.SignInWithLinkedAccount("google", "123", new[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com"),
                new Claim(ClaimTypes.Name, @" ~ Christian.Forrest-Smith_OK @ "),
                new Claim(ClaimTypes.GivenName, "Christian")
            });
            var addedAccount = (MyUserAccount)repository.UserAccounts[0];
            Assert.AreEqual("Christian", addedAccount.FirstName);
        }

        [TestMethod]
        public void Claims_Mapped_Account_Properties_Should_Be_Available_In_AccountCreatedEvent()
        {
            subject.SignInWithLinkedAccount("google", "123", new[]
            {
                new Claim(ClaimTypes.Email, "test@gmail.com"),
                new Claim(ClaimTypes.Name, @" ~ Christian.Forrest-Smith_OK @ "),
                new Claim(ClaimTypes.GivenName, "Christian")
            });
            var account = (MyUserAccount)accountCreatedEvent.Latest.Account;
            Assert.AreEqual("Christian", account.FirstName);
        }
    }
}
