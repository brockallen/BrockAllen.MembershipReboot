/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using Owin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot.Owin
{
    public class MembershipRebootMiddleware<TAccount>
        where TAccount : UserAccount
    {
        Func<IDictionary<string, object>, Task> next;
        Func<IDictionary<string, object>, UserAccountService<TAccount>> userAccountServiceFactory;
        Func<IDictionary<string, object>, AuthenticationService<TAccount>> authenticationServiceFactory;

        public MembershipRebootMiddleware(
            Func<IDictionary<string, object>, Task> next, 
            Func<IDictionary<string, object>, UserAccountService<TAccount>> userAccountServiceFactory,
            Func<IDictionary<string, object>, AuthenticationService<TAccount>> authenticationServiceFactory = null)
        {
            this.next = next;
            this.userAccountServiceFactory = userAccountServiceFactory;
            this.authenticationServiceFactory = authenticationServiceFactory;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            env.SetUserAccountService(() =>
            {
                return this.userAccountServiceFactory(env);
            });
            env.SetAuthenticationService(() =>
            {
                return this.authenticationServiceFactory(env);
            });

            await next(env);
        }
    }
    
    public class MembershipRebootMiddleware : MembershipRebootMiddleware<UserAccount>
    { 
        public MembershipRebootMiddleware(
            Func<IDictionary<string, object>, Task> next,
            Func<IDictionary<string, object>, UserAccountService<UserAccount>> userAccountServiceFactory,
            Func<IDictionary<string, object>, AuthenticationService<UserAccount>> authenticationServiceFactory = null)
            : base(next, userAccountServiceFactory, authenticationServiceFactory)
        {
        }
    }

}