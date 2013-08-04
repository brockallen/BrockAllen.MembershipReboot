/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;
namespace BrockAllen.MembershipReboot
{
    public class DbContextRoleRepository<Ctx>
           : DbContextRepository<Role>, IRoleRepository
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
