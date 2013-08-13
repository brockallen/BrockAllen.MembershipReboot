/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot.Ef
{
    public class DefaultRoleRepository
        : DbContextRoleRepository<DefaultMembershipRebootDatabase>
    {
        public DefaultRoleRepository()
        {
        }

        public DefaultRoleRepository(string name)
            : base(new DefaultMembershipRebootDatabase(name))
        {
        }
    }
}
