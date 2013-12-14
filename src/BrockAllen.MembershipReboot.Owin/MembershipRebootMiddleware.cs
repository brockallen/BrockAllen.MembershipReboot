/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot;
using BrockAllen.MembershipReboot.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Owin;

namespace BrockAllen.MembershipReboot.Owin
{
    public class MembershipRebootMiddleware<TAccount>
        where TAccount : UserAccount
    {
        Func<IDictionary<string, object>, Task> next;
        Func<IOwinContext, UserAccountService<TAccount>> userAccountServiceFactory;
        Func<IOwinContext, AuthenticationService<TAccount>> authenticationServiceFactory;

        public MembershipRebootMiddleware(
            Func<IDictionary<string, object>, Task> next, 
            Func<IOwinContext, UserAccountService<TAccount>> userAccountServiceFactory,
            Func<IOwinContext, AuthenticationService<TAccount>> authenticationServiceFactory = null)
        {
            this.next = next;
            this.userAccountServiceFactory = userAccountServiceFactory;
            this.authenticationServiceFactory = authenticationServiceFactory;
        }

        public async Task Invoke(IDictionary<string, object> env)
        {
            var ctx = new OwinContext(env);

            ctx.SetUserAccountService(() =>
            {
                return this.userAccountServiceFactory(ctx);
            });
            ctx.SetAuthenticationService(() =>
            {
                return this.authenticationServiceFactory(ctx);
            });

            await next(env);
        }
    }
    
    public class MembershipRebootMiddleware : MembershipRebootMiddleware<UserAccount>
    { 
        public MembershipRebootMiddleware(
            Func<IDictionary<string, object>, Task> next,
            Func<IOwinContext, UserAccountService<UserAccount>> userAccountServiceFactory,
            Func<IOwinContext, AuthenticationService<UserAccount>> authenticationServiceFactory = null)
            : base(next, userAccountServiceFactory, authenticationServiceFactory)
        {
        }
    }

}