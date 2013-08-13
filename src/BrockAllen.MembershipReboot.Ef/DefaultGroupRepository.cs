/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultGroupRepository
        : DbContextGroupRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultGroupRepository()
        {
        }

        public DefaultGroupRepository(string name)
            : base(new DefaultMembershipRebootDatabase(name))
        {
        }
    }
}
