/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        public virtual void Process(UserAccountEvent<TAccount> evt, object extra = null)
        {
            var data = new Dictionary<string, string>();
            if (extra != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(extra))
                {
                    object obj2 = descriptor.GetValue(extra);
                    if (obj2 != null)
                    {
                        data.Add(descriptor.Name, obj2.ToString());
                    }
                }
            }

            var msg = CreateMessage(evt, data);
            if (msg != null)
            {
                SendSms(msg);
            }
        }

        protected virtual Message CreateMessage(UserAccountEvent<TAccount> evt, IDictionary<string, string> extra)
        {
            var msg = this.messageFormatter.Format(evt, extra);
            if (msg != null)
            {
                if (extra.ContainsKey("NewMobilePhoneNumber"))
                {
                    msg.To = extra["NewMobilePhoneNumber"];
                }
                else
                {
                    msg.To = evt.Account.MobilePhoneNumber;
                }
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
