/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Configuration;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class SecuritySettings : ConfigurationSection
    {
        public static SecuritySettings Instance { get; set; }

        static SecuritySettings()
        {
            Instance = FromConfiguration();
        }

        public static SecuritySettings FromConfiguration()
        {
            var instance = new SecuritySettings();
            var configSection = GetConfigSection();
            if (configSection != null)
            {
                foreach (var prop in configSection.Properties.Cast<ConfigurationProperty>())
                {
                    var val = configSection[prop];
                    instance[prop] = val;
                }
            }
            return instance;
        }

        public const string SectionName = "membershipReboot";

        static SecuritySettings GetConfigSection()
        {
            return (SecuritySettings)System.Configuration.ConfigurationManager.GetSection(SectionName);
        }

        private const string MULTITENANT = "multiTenant";
        private const string DEFAULTTENANT = "defaultTenant";
        private const string EMAILISUSERNAME = "emailIsUsername";
        private const string EMAILISUNIQUE = "emailIsUnique";
        private const string USERNAMESUNIQUEACROSSTENANTS = "usernamesUniqueAcrossTenants";
        private const string REQUIREACCOUNTVERIFICATION = "requireAccountVerification";
        private const string ALLOWLOGINAFTERACCOUNTCREATION = "allowLoginAfterAccountCreation";
        private const string ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS = "accountLockoutFailedLoginAttempts";
        private const string ACCOUNTLOCKOUTDURATION = "accountLockoutDuration";
        private const string ALLOWACCOUNTDELETION = "allowAccountDeletion";
        private const string PASSWORDHASHINGITERATIONCOUNT = "passwordHashingIterationCount";
        private const string PASSWORDRESETFREQUENCY = "passwordResetFrequency";
        private const string VERIFICATIONKEYLIFETIME = "verificationKeyLifetime";
        private const string CERTIFICATEISUNIQUE = "certificateIsUnique";
        private const string MOBILEPHONEISUNIQUE = "mobilePhoneIsUnique";
        private const string REQUIREPASSWORDRESETAFTERACCOUNTCREATION = "requirePasswordResetAfterAccountCreation";

        [ConfigurationProperty(MULTITENANT, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.MultiTenant)]
        public bool MultiTenant
        {
            get { return (bool)this[MULTITENANT]; }
            set { this[MULTITENANT] = value; }
        }

        [ConfigurationProperty(DEFAULTTENANT, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.DefaultTenant)]
        public string DefaultTenant
        {
            get { return (string)this[DEFAULTTENANT]; }
            set { this[DEFAULTTENANT] = value; }
        }

        [ConfigurationProperty(EMAILISUSERNAME, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.EmailIsUsername)]
        public bool EmailIsUsername
        {
            get { return (bool)this[EMAILISUSERNAME]; }
            set { this[EMAILISUSERNAME] = value; }
        }
        
        [ConfigurationProperty(EMAILISUNIQUE, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.EmailIsUnique)]
        public bool EmailIsUnique
        {
            get { return (bool)this[EMAILISUNIQUE]; }
            set { this[EMAILISUNIQUE] = value; }
        }

        [ConfigurationProperty(USERNAMESUNIQUEACROSSTENANTS, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.UsernamesUniqueAcrossTenants)]
        public bool UsernamesUniqueAcrossTenants
        {
            get { return (bool)this[USERNAMESUNIQUEACROSSTENANTS]; }
            set { this[USERNAMESUNIQUEACROSSTENANTS] = value; }
        }

        [ConfigurationProperty(REQUIREACCOUNTVERIFICATION, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.RequireAccountVerification)]
        public bool RequireAccountVerification
        {
            get { return (bool)this[REQUIREACCOUNTVERIFICATION]; }
            set { this[REQUIREACCOUNTVERIFICATION] = value; }
        }

        [ConfigurationProperty(ALLOWLOGINAFTERACCOUNTCREATION, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.AllowLoginAfterAccountCreation)]
        public bool AllowLoginAfterAccountCreation
        {
            get { return (bool)this[ALLOWLOGINAFTERACCOUNTCREATION]; }
            set { this[ALLOWLOGINAFTERACCOUNTCREATION] = value; }
        }

        [ConfigurationProperty(ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.AccountLockoutFailedLoginAttempts)]
        public int AccountLockoutFailedLoginAttempts
        {
            get { return (int)this[ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS]; }
            set { this[ACCOUNTLOCKOUTFAILEDLOGINATTEMPTS] = value; }
        }

        [ConfigurationProperty(ACCOUNTLOCKOUTDURATION, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.AccountLockoutDuration)]
        public TimeSpan AccountLockoutDuration
        {
            get { return (TimeSpan)this[ACCOUNTLOCKOUTDURATION]; }
            set { this[ACCOUNTLOCKOUTDURATION] = value; }
        }

        [ConfigurationProperty(ALLOWACCOUNTDELETION, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.AllowAccountDeletion)]
        public bool AllowAccountDeletion
        {
            get { return (bool)this[ALLOWACCOUNTDELETION]; }
            set { this[ALLOWACCOUNTDELETION] = value; }
        }

        [ConfigurationProperty(PASSWORDHASHINGITERATIONCOUNT, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.PasswordHashingIterationCount)]
        public int PasswordHashingIterationCount
        {
            get { return (int)this[PASSWORDHASHINGITERATIONCOUNT]; }
            set { this[PASSWORDHASHINGITERATIONCOUNT] = value; }
        }

        [ConfigurationProperty(PASSWORDRESETFREQUENCY, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.PasswordResetFrequency)]
        public int PasswordResetFrequency
        {
            get { return (int)this[PASSWORDRESETFREQUENCY]; }
            set { this[PASSWORDRESETFREQUENCY] = value; }
        }

        [ConfigurationProperty(VERIFICATIONKEYLIFETIME, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.VerificationKeyLifetime)]
        public TimeSpan VerificationKeyLifetime
        {
            get { return (TimeSpan)this[VERIFICATIONKEYLIFETIME]; }
            set { this[VERIFICATIONKEYLIFETIME] = value; }
        }

        [ConfigurationProperty(CERTIFICATEISUNIQUE, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.CertificateIsUnique)]
        public bool CertificateIsUnique
        {
            get { return (bool) this[CERTIFICATEISUNIQUE]; }
            set { this[CERTIFICATEISUNIQUE] = value; }
        }

        [ConfigurationProperty(MOBILEPHONEISUNIQUE, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.MobilePhoneIsUnique)]
        public bool MobilePhoneIsUnique
        {
            get { return (bool) this[MOBILEPHONEISUNIQUE]; }
            set { this[MOBILEPHONEISUNIQUE] = value; }
        }

        [ConfigurationProperty(REQUIREPASSWORDRESETAFTERACCOUNTCREATION, DefaultValue = MembershipRebootConstants.SecuritySettingDefaults.RequirePasswordResetAfterAccountCreation)]
        public bool RequirePasswordResetAfterAccountCreation
        {
            get { return (bool) this[REQUIREPASSWORDRESETAFTERACCOUNTCREATION]; }
            set { this[REQUIREPASSWORDRESETAFTERACCOUNTCREATION] = value; }
        }
    }
}
