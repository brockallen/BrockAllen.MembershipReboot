/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

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

        public virtual void Process(UserAccountEvent evt, object data = null)
        {
            dynamic d = new DynamicDictionary(data);
            var msg = CreateMessage(evt, d);
            if (msg != null)
            {
                SendSms(msg);
            }
        }
        
        protected virtual Message CreateMessage(UserAccountEvent evt, dynamic extra)
        {
            var msg = this.messageFormatter.Format(evt, extra);
            if (msg != null)
            {
                msg.To = extra.NewMobilePhoneNumber ?? evt.Account.MobilePhoneNumber;
            }
            return msg;
        }

        public void Handle(MobilePhoneChangeRequestedEvent evt)
        {
            Process(evt, new { evt.NewMobilePhoneNumber, evt.Code });
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent evt)
        {
            Process(evt, new { evt.Code });
        }
    }
}
