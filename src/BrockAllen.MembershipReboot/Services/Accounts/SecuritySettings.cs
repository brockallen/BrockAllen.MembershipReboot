using System;
using System.Configuration;

namespace BrockAllen.MembershipReboot
{
    public class SecuritySettings : ConfigurationSection
    {
        public static SecuritySettings Instance { get; set; }

        static SecuritySettings()
        {
            var configSection = GetConfigSection();
            if (configSection == null)
            {
                configSection = new SecuritySettings();
            }
            Instance = configSection;
        }

        public const string SectionName = "membershipReboot";

        static SecuritySettings GetConfigSection()
        {
            return (SecuritySettings)System.Configuration.ConfigurationManager.GetSection(SectionName);
        }

        private const string CONNECTIONSTRINGNAME = "connectionStringName";
        private const string MULTITENANT = "multiTenant";
        private const string DEFAULTTENANT = "defaultTenant";
        private const string EMAILISUSERNAME = "emailIsUsername";
        private const string ALLOWEMAILCHANGEWHENEMAILISUSERNAME = "allowEmailChangeWhenEmailIsUsername";
        private const string USERNAMESUNIQUEACROSSTENANTS = "usernamesUniqueAcrossTenants";
        private const string REQUIREACCOUNTVERIFICATION = "requireAccountVerification";
        private const string ALLOWLOGINAFTERACCOUNTCREATION = "allowLoginAfterAccountCreation";
        private const string ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS = "accountLockoutFailedLoginAttempts";
        private const string ACCOUNTLOCKOUTDURATION = "accountLockoutDuration";
        private const string ALLOWACCOUNTDELETION = "allowAccountDeletion";
        private const string PASSWORDHASHINGITERATIONCOUNT = "passwordHashingIterationCount";
        private const string PASSWORDRESETFREQUENCY = "passwordResetFrequency";

        [ConfigurationProperty(CONNECTIONSTRINGNAME, DefaultValue = "MembershipReboot")]
        public string ConnectionStringName
        {
            get { return (string)this[CONNECTIONSTRINGNAME]; }
            set { this[CONNECTIONSTRINGNAME] = value; }
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

        [ConfigurationProperty(ALLOWEMAILCHANGEWHENEMAILISUSERNAME, DefaultValue = false)]
        public bool AllowEmailChangeWhenEmailIsUsername
        {
            get { return (bool)this[ALLOWEMAILCHANGEWHENEMAILISUSERNAME]; }
            set { this[ALLOWEMAILCHANGEWHENEMAILISUSERNAME] = value; }
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

        [ConfigurationProperty(ACCOUNTLOCKOUTDURATION, DefaultValue="00:05:00")]
        public TimeSpan AccountLockoutDuration
        {
            get { return (TimeSpan)this[ACCOUNTLOCKOUTDURATION]; }
            set { this[ACCOUNTLOCKOUTDURATION] = value; }
        }

        [ConfigurationProperty(ALLOWACCOUNTDELETION, DefaultValue = true)]
        public bool AllowAccountDeletion
        {
            get { return (bool)this[ALLOWACCOUNTDELETION]; }
            set { this[ALLOWACCOUNTDELETION] = value; }
        }

        [ConfigurationProperty(PASSWORDHASHINGITERATIONCOUNT, DefaultValue = 0)]
        public int PasswordHashingIterationCount
        {
            get { return (int)this[PASSWORDHASHINGITERATIONCOUNT]; }
            set { this[PASSWORDHASHINGITERATIONCOUNT] = value; }
        }

        [ConfigurationProperty(PASSWORDRESETFREQUENCY, DefaultValue = 0)]
        public int PasswordResetFrequency
        {
            get { return (int)this[PASSWORDRESETFREQUENCY]; }
            set { this[PASSWORDRESETFREQUENCY] = value; }
        }
    }
}
