using System;
using System.Linq;
using BrockAllen.MembershipReboot.Helpers;

namespace BrockAllen.MembershipReboot
{
    internal static class CryptoHelper
    {
        internal const char PasswordHashingIterationCountSeparator = '.';

        internal static string Hash(string value)
        {
            return Crypto.Hash(value);
        }
        
        internal static string GenerateSalt()
        {
            return Crypto.GenerateSalt();
        }

        internal static string HashPassword(string password)
        {
            var count = SecuritySettings.Instance.PasswordHashingIterationCount;
            if (count <= 0)
            {
                return Crypto.HashPassword(password);
            }
            else
            {
                var result = Crypto.HashPassword(password, count);
                return count.ToString() + PasswordHashingIterationCountSeparator + result;
            }
        }

        internal static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword.Contains(PasswordHashingIterationCountSeparator))
            {
                var parts = hashedPassword.Split(PasswordHashingIterationCountSeparator);
                if (parts.Length != 2) return false;
                
                int count;
                if (!Int32.TryParse(parts[0], out count)) return false;

                hashedPassword = parts[1];
                
                return Crypto.VerifyHashedPassword(hashedPassword, password, count);
            }
            else
            {
                return Crypto.VerifyHashedPassword(hashedPassword, password);
            }
        }
    }
}
