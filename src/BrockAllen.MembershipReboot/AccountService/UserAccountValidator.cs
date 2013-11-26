/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace BrockAllen.MembershipReboot
{
    class UserAccountValidator<T> :
        IEventHandler<CertificateAddedEvent<T>>,
        IEventHandler<MobilePhoneChangeRequestedEvent<T>>,
        IEventHandler<MobilePhoneChangedEvent<T>>
        where T : UserAccount
    {
        UserAccountService<T> userAccountService;
        public UserAccountValidator(UserAccountService<T> userAccountService)
        {
            if (userAccountService == null) throw new ArgumentNullException("userAccountService");
            this.userAccountService = userAccountService;
        }

        public void Handle(CertificateAddedEvent<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("event");
            if (evt.Account == null) throw new ArgumentNullException("account");
            if (evt.Certificate == null) throw new ArgumentNullException("certificate");

            var account = evt.Account;
            var otherAccount = userAccountService.GetByCertificate(account.Tenant, evt.Certificate.Thumbprint);
            if (otherAccount != null && otherAccount.ID != account.ID)
            {
                Tracing.Verbose("[UserAccountValidation.CertificateThumbprintMustBeUnique] validation failed: {0}, {1}", account.Tenant, account.Username);
                throw new ValidationException(Resources.ValidationMessages.CertificateAlreadyInUse);
            }
        }

        public void Handle(MobilePhoneChangeRequestedEvent<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("event");
            if (evt.Account == null) throw new ArgumentNullException("account");
            if (String.IsNullOrWhiteSpace(evt.NewMobilePhoneNumber)) throw new ArgumentNullException("NewMobilePhoneNumber");

            ValidateMobileNumber(evt.Account, evt.NewMobilePhoneNumber);
        }

        public void Handle(MobilePhoneChangedEvent<T> evt)
        {
            if (evt == null) throw new ArgumentNullException("event");
            if (evt.Account == null) throw new ArgumentNullException("account");
            
            ValidateMobileNumber(evt.Account, evt.Account.MobilePhoneNumber);
        }

        void ValidateMobileNumber(UserAccount account, string mobile)
        {
            if (!String.IsNullOrWhiteSpace(mobile))
            {
                var query =
                    from a in userAccountService.GetAll(account.Tenant)
                    where a.MobilePhoneNumber == mobile && a.ID != account.ID
                    select a;

                if (query.Any())
                {
                    Tracing.Verbose("[UserAccountValidation.MobilePhoneMustBeUnique] validation failed: {0}, {1}", account.Tenant, account.Username);
                    throw new ValidationException(Resources.ValidationMessages.MobilePhoneAlreadyInUse);
                }
            }
        }
    }
}
