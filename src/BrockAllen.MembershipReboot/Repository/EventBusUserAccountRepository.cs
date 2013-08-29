/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */
using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    public class EventBusUserAccountRepository :
       EventBusRepository<UserAccount>, IUserAccountRepository
    {
        IUserAccountRepository inner;

        public EventBusUserAccountRepository(IUserAccountRepository inner, IEventBus validationBus, IEventBus eventBus)
            : base(inner, validationBus, eventBus)
        {
            this.inner = inner;
        }

        public UserAccount FindByLinkedAccount(string tenant, string provider, string id) 
        {
            return inner.FindByLinkedAccount(tenant, provider, id);  
        }
    }
}
