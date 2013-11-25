/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultUserAccountRepository
           : DbContextUserAccountRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultUserAccountRepository()
        {
        }

        public DefaultUserAccountRepository(string name)
            : base(new DefaultMembershipRebootDatabase(name))
        {
        }
    }
}
