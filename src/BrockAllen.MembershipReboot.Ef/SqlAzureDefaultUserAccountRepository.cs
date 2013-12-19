/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


namespace BrockAllen.MembershipReboot.Ef
{
    public class SqlAzureDefaultUserAccountRepository
           : DbContextUserAccountRepository<SqlAzureMembershipRebootDatabase, UserAccount>, IUserAccountRepository
    {
        public SqlAzureDefaultUserAccountRepository()
        {
        }

        public SqlAzureDefaultUserAccountRepository(string name)
            : base(new SqlAzureMembershipRebootDatabase(name))
        {
        }
    }
}
