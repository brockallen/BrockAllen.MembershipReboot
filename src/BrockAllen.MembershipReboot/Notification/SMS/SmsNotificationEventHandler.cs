/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public abstract class SmsEventHandler : SmsEventHandler<UserAccount>
    {
        public SmsEventHandler(IMessageFormatter<UserAccount> messageFormatter)
            : base(messageFormatter)
        {
        }
    }

    public abstract class SmsEventHandler<T> :
        IEventHandler<MobilePhoneChangeRequestedEvent<T>>,
        IEventHandler<TwoFactorAuthenticationCodeNotificationEvent<T>>
        where T: UserAccount
    {
        IMessageFormatter<T> messageFormatter;

        public SmsEventHandler(IMessageFormatter<T> messageFormatter)
        {
            if (messageFormatter == null) throw new ArgumentNullException("messageFormatter");

            this.messageFormatter = messageFormatter;
        }

        protected abstract void SendSms(Message message);

        public virtual void Process(UserAccountEvent<T> evt, object data = null)
        {
            dynamic d = new DynamicDictionary(data);
            var msg = CreateMessage(evt, d);
            if (msg != null)
            {
                SendSms(msg);
            }
        }

        protected virtual Message CreateMessage(UserAccountEvent<T> evt, dynamic extra)
        {
            var msg = this.messageFormatter.Format(evt, extra);
            if (msg != null)
            {
                msg.To = extra.NewMobilePhoneNumber ?? evt.Account.MobilePhoneNumber;
            }
            return msg;
        }

        public void Handle(MobilePhoneChangeRequestedEvent<T> evt)
        {
            Process(evt, new { evt.NewMobilePhoneNumber, evt.Code });
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent<T> evt)
        {
            Process(evt, new { evt.Code });
        }
    }
}
