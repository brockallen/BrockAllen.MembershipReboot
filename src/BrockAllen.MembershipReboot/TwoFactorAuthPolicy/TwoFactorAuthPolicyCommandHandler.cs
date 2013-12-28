/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;

namespace BrockAllen.MembershipReboot
{
    public class TwoFactorAuthPolicyCommandHandler :
        ICommandHandler<GetTwoFactorAuthToken>,
        ICommandHandler<IssueTwoFactorAuthToken>,
        ICommandHandler<ClearTwoFactorAuthToken>
    {
        ITwoFactorAuthenticationPolicy policy;
        public TwoFactorAuthPolicyCommandHandler(ITwoFactorAuthenticationPolicy policy)
        {
            if (policy == null) throw new ArgumentNullException("policy");
            this.policy = policy;
        }

        public void Handle(GetTwoFactorAuthToken cmd)
        {
            cmd.Token = policy.GetTwoFactorAuthToken(cmd.Account);
        }

        public void Handle(IssueTwoFactorAuthToken cmd)
        {
            policy.IssueTwoFactorAuthToken(cmd.Account, cmd.Token);
            cmd.Success = true;
        }

        public void Handle(ClearTwoFactorAuthToken cmd)
        {
            policy.ClearTwoFactorAuthToken(cmd.Account);
        }
    }
}
