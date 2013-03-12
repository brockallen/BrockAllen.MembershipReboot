using System;
using System.Configuration;

namespace BrockAllen.MembershipReboot
{
    public class SecuritySettings : ConfigurationSection
    {
        public const string SectionName = "membershipReboot";

        private const string MULTITENANT = "multiTenant";
        private const string DEFAULTTENANT = "defaultTenant";
        private const string EMAILISUSERNAME = "emailIsUsername";
        private const string USERNAMESUNIQUEACROSSTENANTS = "usernamesUniqueAcrossTenants";
        private const string REQUIREACCOUNTVERIFICATION = "requireAccountVerification";
        private const string ALLOWLOGINAFTERACCOUNTCREATION = "allowLoginAfterAccountCreation";
        private const string ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS = "accountLockoutFailedLoginAttempts";
        private const string ACCOUNTLOCKOUTDURATION = "accountLockoutDuration";
        private const string ALLOWACCOUNTDELETION = "allowAccountDeletion";
        private const string CONNECTIONSTRING = "connectionString";

        public static SecuritySettings Instance
        {
            get { return (SecuritySettings)System.Configuration.ConfigurationManager.GetSection(SectionName); }
        }

        [ConfigurationProperty(MULTITENANT, DefaultValue = false)]
        public bool MultiTenant
        {
            get { return (bool)this[MULTITENANT]; }
            set { this[MULTITENANT] = value; }
        }

        [ConfigurationProperty(DEFAULTTENANT, DefaultValue = "default")]
        public string DefaultTenant
        {
            get { return (string)this[DEFAULTTENANT]; }
            set { this[DEFAULTTENANT] = value; }
        }

        [ConfigurationProperty(EMAILISUSERNAME, DefaultValue = false)]
        public bool EmailIsUsername
        {
            get { return (bool)this[EMAILISUSERNAME]; }
            set { this[EMAILISUSERNAME] = value; }
        }

        [ConfigurationProperty(USERNAMESUNIQUEACROSSTENANTS, DefaultValue = false)]
        public bool UsernamesUniqueAcrossTenants
        {
            get { return (bool)this[USERNAMESUNIQUEACROSSTENANTS]; }
            set { this[USERNAMESUNIQUEACROSSTENANTS] = value; }
        }

        [ConfigurationProperty(REQUIREACCOUNTVERIFICATION, DefaultValue = true)]
        public bool RequireAccountVerification
        {
            get { return (bool)this[REQUIREACCOUNTVERIFICATION]; }
            set { this[REQUIREACCOUNTVERIFICATION] = value; }
        }

        [ConfigurationProperty(ALLOWLOGINAFTERACCOUNTCREATION, DefaultValue = true)]
        public bool AllowLoginAfterAccountCreation
        {
            get { return (bool)this[ALLOWLOGINAFTERACCOUNTCREATION]; }
            set { this[ALLOWLOGINAFTERACCOUNTCREATION] = value; }
        }

        [ConfigurationProperty(ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS, DefaultValue = 10)]
        public int AccountLockoutFailedLoginAttempts
        {
            get { return (int)this[ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS]; }
            set { this[ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS] = value; }
        }

        public TimeSpan AccountLockoutDuration
        {
            get { return TimeSpan.FromMinutes(AccountLockoutMinutes); }
            set { AccountLockoutMinutes = (int)value.TotalMinutes; }
        }

        [ConfigurationProperty(MULTITENANT, DefaultValue = false)]
        public int AccountLockoutMinutes
        {
            get { return (int)this[ACCOUNTLOCKOUTDURATION]; }
            set { this[ACCOUNTLOCKOUTDURATION] = value; }
        }

        [ConfigurationProperty(ALLOWACCOUNTDELETION, DefaultValue = true)]
        public bool AllowAccountDeletion
        {
            get { return (bool)this[ALLOWACCOUNTDELETION]; }
            set { this[ALLOWACCOUNTDELETION] = value; }
        }

        [ConfigurationProperty(CONNECTIONSTRING, DefaultValue = "MembershipReboot")]
        public string ConnectionString
        {
            get { return (string)this[CONNECTIONSTRING]; }
            set { this[CONNECTIONSTRING] = value; }
        }
    }
}
