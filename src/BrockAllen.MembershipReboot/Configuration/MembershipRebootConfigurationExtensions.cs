using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    static class MembershipRebootConfigurationExtensions
    {
        public static void FromLegacy(
            this MembershipRebootConfiguration config, 
            IUserAccountRepository userAccountRepository, 
            INotificationService notificationService, 
            IPasswordPolicy passwordPolicy)
        {
            if (config == null) throw new ArgumentNullException("config");
            
            config.SetUserAccountRepository(userAccountRepository);

            if (notificationService != null)
            {
                config.AddEventHandler(new NotificationServiceEventHandler(notificationService));
                if (config.SecuritySettings.RequireAccountVerification)
                {
                    config.AddEventHandler(new NotificationServiceAccountCreatedEventHandler(notificationService));
                }
            }

            if (passwordPolicy != null)
            {
                config.RegisterPasswordValidator(new DelegateValidator(
                    (svc, acct, password) =>
                    {
                        if (!passwordPolicy.ValidatePassword(password))
                        {
                            return new ValidationResult("Invalid password: " + passwordPolicy.PolicyMessage);
                        }
                        return null;
                    }));
            }
        }
    }
}
