/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextGroupRepository<Ctx>
           : DbContextRepository<Group>, IGroupRepository
        where Ctx : DbContext, new()
    {
        public DbContextGroupRepository()
            : this(new Ctx())
        {
        }
        public DbContextGroupRepository(Ctx ctx)
            : base(ctx)
        {
        }
    }
}
