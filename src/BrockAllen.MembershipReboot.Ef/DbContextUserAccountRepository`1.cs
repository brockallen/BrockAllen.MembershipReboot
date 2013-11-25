/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextUserAccountRepository<Ctx>
           : DbContextRepository<UserAccount>, IUserAccountRepository
        where Ctx : DbContext, new()
    {
        public DbContextUserAccountRepository()
            : this(new Ctx())
        {
        }
        public DbContextUserAccountRepository(Ctx ctx)
            : base(ctx)
        {
        }
    }
}
