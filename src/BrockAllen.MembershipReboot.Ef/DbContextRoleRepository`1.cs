/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextRoleRepository<Ctx>
           : DbContextRepository<Group>, IGroupRepository
        where Ctx : DbContext, new()
    {
        public DbContextRoleRepository()
            : this(new Ctx())
        {
        }
        public DbContextRoleRepository(Ctx ctx)
            : base(ctx)
        {
        }
    }
}
