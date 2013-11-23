/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;

namespace BrockAllen.MembershipReboot
{
    public class EmailEventHandler
    {
        IMessageFormatter messageFormatter;
        IMessageDelivery messageDelivery;

        public EmailEventHandler(IMessageFormatter messageFormatter)
            : this(messageFormatter, new SmtpMessageDelivery())
        {
        }
        
        public EmailEventHandler(IMessageFormatter messageFormatter, IMessageDelivery messageDelivery)
        {
            if (messageFormatter == null) throw new ArgumentNullException("messageFormatter");
            if (messageDelivery == null) throw new ArgumentNullException("messageDelivery");

            this.messageFormatter = messageFormatter;
            this.messageDelivery = messageDelivery;
        }

        public virtual void Process(UserAccountEvent evt, object extra = null)
        {
            dynamic d = new DynamicDictionary(extra);
            var msg = this.messageFormatter.Format(evt, d);
            if (msg != null)
            {
                msg.To = d.NewEmail ?? evt.Account.Email;
                this.messageDelivery.Send(msg);
            }
        }
    }

    public class EmailAccountCreatedEventHandler
        : EmailEventHandler, IEventHandler<AccountCreatedEvent>
    {
        public EmailAccountCreatedEventHandler(IMessageFormatter messageFormatter)
            : base(messageFormatter)
        {
        }
        
        public EmailAccountCreatedEventHandler(IMessageFormatter messageFormatter, IMessageDelivery messageDelivery)
            : base(messageFormatter, messageDelivery)
        {
        }

        public void Handle(AccountCreatedEvent evt)
        {
            Process(evt, new { evt.VerificationKey });
        }
    }

    public class EmailAccountEventsHandler :
        EmailEventHandler,
        IEventHandler<AccountVerifiedEvent>,
        IEventHandler<PasswordResetRequestedEvent>,
        IEventHandler<PasswordChangedEvent>,
        IEventHandler<UsernameReminderRequestedEvent>,
        IEventHandler<AccountClosedEvent>,
        IEventHandler<UsernameChangedEvent>,
        IEventHandler<EmailChangeRequestedEvent>,
        IEventHandler<EmailChangedEvent>,
        IEventHandler<MobilePhoneChangedEvent>,
        IEventHandler<MobilePhoneRemovedEvent>,
        IEventHandler<CertificateAddedEvent>,
        IEventHandler<CertificateRemovedEvent>,
        IEventHandler<LinkedAccountAddedEvent>,
        IEventHandler<LinkedAccountRemovedEvent>
    {
        public EmailAccountEventsHandler(IMessageFormatter messageFormatter)
            : base(messageFormatter)
        {
        }
        public EmailAccountEventsHandler(IMessageFormatter messageFormatter, IMessageDelivery messageDelivery)
            : base(messageFormatter, messageDelivery)
        {
        }

        public void Handle(AccountVerifiedEvent evt)
        {
            Process(evt);
        }

        public void Handle(PasswordResetRequestedEvent evt)
        {
            Process(evt, new { evt.VerificationKey });
        }

        public void Handle(PasswordChangedEvent evt)
        {
            Process(evt);
        }

        public void Handle(UsernameReminderRequestedEvent evt)
        {
            Process(evt);
        }

        public void Handle(AccountClosedEvent evt)
        {
            Process(evt);
        }

        public void Handle(UsernameChangedEvent evt)
        {
            Process(evt);
        }

        public void Handle(EmailChangeRequestedEvent evt)
        {
            Process(evt, new{evt.NewEmail, evt.VerificationKey});
        }

        public void Handle(EmailChangedEvent evt)
        {
            Process(evt);
        }

        public void Handle(MobilePhoneChangedEvent evt)
        {
            Process(evt);
        }

        public void Handle(MobilePhoneRemovedEvent evt)
        {
            Process(evt);
        }

        public void Handle(CertificateAddedEvent evt)
        {
            Process(evt);
        }

        public void Handle(CertificateRemovedEvent evt)
        {
            Process(evt);
        }

        public void Handle(LinkedAccountAddedEvent evt)
        {
            Process(evt);
        }

        public void Handle(LinkedAccountRemovedEvent evt)
        {
            Process(evt);
        }
    }
}
