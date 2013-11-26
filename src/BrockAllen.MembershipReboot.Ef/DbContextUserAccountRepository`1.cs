/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System.Data.Entity;
namespace BrockAllen.MembershipReboot.Ef
{
    public class DbContextUserAccountRepository<Ctx, T>
           : DbContextRepository<T>, IUserAccountRepository<T>
        where Ctx : DbContext, new()
        where T : UserAccount
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
