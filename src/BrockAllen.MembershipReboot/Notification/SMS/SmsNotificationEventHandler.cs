/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public abstract class SmsEventHandler<TAccount> :
        IEventHandler<MobilePhoneChangeRequestedEvent<TAccount>>,
        IEventHandler<TwoFactorAuthenticationCodeNotificationEvent<TAccount>>
        where TAccount: UserAccount
    {
        IMessageFormatter<TAccount> messageFormatter;

        public SmsEventHandler(IMessageFormatter<TAccount> messageFormatter)
        {
            if (messageFormatter == null) throw new ArgumentNullException("messageFormatter");

            this.messageFormatter = messageFormatter;
        }

        protected abstract void SendSms(Message message);

        public virtual void Process(UserAccountEvent<TAccount> evt, object data = null)
        {
            dynamic d = new DynamicDictionary(data);
            var msg = CreateMessage(evt, d);
            if (msg != null)
            {
                SendSms(msg);
            }
        }

        protected virtual Message CreateMessage(UserAccountEvent<TAccount> evt, dynamic extra)
        {
            var msg = this.messageFormatter.Format(evt, extra);
            if (msg != null)
            {
                msg.To = extra.NewMobilePhoneNumber ?? evt.Account.MobilePhoneNumber;
            }
            return msg;
        }

        public void Handle(MobilePhoneChangeRequestedEvent<TAccount> evt)
        {
            Process(evt, new { evt.NewMobilePhoneNumber, evt.Code });
        }

        public void Handle(TwoFactorAuthenticationCodeNotificationEvent<TAccount> evt)
        {
            Process(evt, new { evt.Code });
        }
    }
    
    public abstract class SmsEventHandler : SmsEventHandler<UserAccount>
    {
        public SmsEventHandler(IMessageFormatter<UserAccount> messageFormatter)
            : base(messageFormatter)
        {
        }
    }
}
