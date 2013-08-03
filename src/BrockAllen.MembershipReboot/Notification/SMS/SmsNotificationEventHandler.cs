/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public abstract class SmsEventHandler :
        IEventHandler<MobilePhoneChangeRequestedEvent>,
        IEventHandler<TwoFactorAuthenticationCodeNotificationEvent>
    {
        IMessageFormatter messageFormatter;

        public SmsEventHandler(IMessageFormatter messageFormatter)
        {
            if (messageFormatter == null) throw new ArgumentNullException("messageFormatter");

            this.messageFormatter = messageFormatter;
        }

        protected abstract void SendSms(Message message);

        public virtual void Process(UserAccountEvent evt, string phone = null)
        {
            var msg = CreateMessage(evt, phone);
            if (msg != null)
            {
                SendSms(msg);
            }
        }
        
        protected virtual Message CreateMessage(UserAccountEvent evt, string phone)
        {
            var msg = this.messageFormatter.Format(evt);
            if (msg != null)
            {
                msg.To = phone ?? evt.Account.MobilePhoneNumber;
            }
            return msg;
        }

        public void Handle(MobilePhoneChangeRequestedEvent evt)
        {
            Process(evt, evt.NewMobilePhoneNumber);
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent evt)
        {
            Process(evt);
        }
    }
}
