using System;
using System.Configuration;

namespace BrockAllen.MembershipReboot
{
    public class SecuritySettings
    {
        public static SecuritySettings Instance { get; set; }

        public bool MultiTenant { get; set; }
        public string DefaultTenant { get; set; }
        public bool EmailIsUsername { get; set; }
        public bool AllowEmailChangeWhenEmailIsUsername { get; set; }
        public bool UsernamesUniqueAcrossTenants { get; set; }
        public bool RequireAccountVerification { get; set; }
        public bool AllowLoginAfterAccountCreation { get; set; }
        public int AccountLockoutFailedLoginAttempts { get; set; }
        public TimeSpan AccountLockoutDuration { get; set; }
        public bool AllowAccountDeletion { get; set; }
        public int PasswordHashingIterationCount { get; set; }

        static SecuritySettings()
        {
            Instance = new SecuritySettings();
        }
        
        public SecuritySettings()
        {
            MultiTenant = GetAppSettings("MultiTenant", false);
            DefaultTenant = GetAppSettings("DefaultTenant", "default");
            EmailIsUsername = GetAppSettings("EmailIsUsername", false);
            AllowEmailChangeWhenEmailIsUsername = GetAppSettings("AllowEmailChangeWhenEmailIsUsername", false);
            UsernamesUniqueAcrossTenants = GetAppSettings("UsernamesUniqueAcrossTenants", false);
            RequireAccountVerification = GetAppSettings("RequireAccountVerification", true);
            AllowLoginAfterAccountCreation = GetAppSettings("AllowLoginAfterAccountCreation", true);
            AccountLockoutFailedLoginAttempts = GetAppSettings("AccountLockoutFailedLoginAttempts", 10);
            AccountLockoutDuration = GetAppSettings("AccountLockoutDuration", TimeSpan.FromMinutes(5));
            AllowAccountDeletion = GetAppSettings("AllowAccountDeletion", true);
            PasswordHashingIterationCount = GetAppSettings("PasswordHashingIterationCount", 1000);
        }

        const string AppSettingsPrefix = "membershipReboot:";

        private T GetAppSettings<T>(string name, T defaultValue)
        {
            var val = ConfigurationManager.AppSettings[AppSettingsPrefix + name];
            if (val != null)
            {
                if (typeof(T) == typeof(TimeSpan))
                {
                    var obj = (object)TimeSpan.Parse(val);
                    return (T)obj;
                }
                else
                {
                    return (T)Convert.ChangeType(val, typeof(T));
                }
            }
            return defaultValue;
        }
    }
}
