/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using BrockAllen.MembershipReboot.Helpers;
using System;
using System.Linq;

namespace BrockAllen.MembershipReboot
{
    internal static class CryptoHelper
    {
        internal const char PasswordHashingIterationCountSeparator = '.';
        internal static Func<int> GetCurrentYear = () => DateTime.Now.Year;

        internal static string Hash(string value)
        {
            return Crypto.Hash(value);
        }
        
        internal static string Hash(string value, string key)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("key");

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(key);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            
            var alg = new System.Security.Cryptography.HMACSHA512(keyBytes);
            var hash = alg.ComputeHash(valueBytes);
            
            var result = Crypto.BinaryToHex(hash);
            return result;
        }

        internal static string Hash(string value, string key)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("key");

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(key);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            var alg = new System.Security.Cryptography.HMACSHA512(keyBytes);
            var hash = alg.ComputeHash(valueBytes);

            var result = Crypto.BinaryToHex(hash);
            return result;
        }

        internal static string GenerateNumericCode(int digits)
        {
            // 18 is good size for a long
            if (digits > 18) digits = 18;
            if (digits <= 0) digits = 6;
            
            var bytes = Crypto.GenerateSaltInternal(sizeof(long));
            var val = BitConverter.ToInt64(bytes, 0);
            var mod = (int)Math.Pow(10, digits);
            val %= mod;
            val = Math.Abs(val);
            
            return val.ToString("D" + digits);
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
                count = GetIterationsFromYear(GetCurrentYear());
            }
            var result = Crypto.HashPassword(password, count);
            return EncodeIterations(count) + PasswordHashingIterationCountSeparator + result;
        }

        internal static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword.Contains(PasswordHashingIterationCountSeparator))
            {
                var parts = hashedPassword.Split(PasswordHashingIterationCountSeparator);
                if (parts.Length != 2) return false;

                int count = DecodeIterations(parts[0]);
                if (count <= 0) return false;

                hashedPassword = parts[1];
                
                return Crypto.VerifyHashedPassword(hashedPassword, password, count);
            }
            else
            {
                return Crypto.VerifyHashedPassword(hashedPassword, password);
            }
        }

        internal static string EncodeIterations(int count)
        {
            return count.ToString("X");
        }

        internal static int DecodeIterations(string prefix)
        {
            int val;
            if (Int32.TryParse(prefix, System.Globalization.NumberStyles.HexNumber, null, out val))
            {
                return val;
            }
            return -1;
        }

        // from OWASP : https://www.owasp.org/index.php/Password_Storage_Cheat_Sheet
        const int StartYear = 2000;
        const int StartCount = 1000;
        internal static int GetIterationsFromYear(int year)
        {
            if (year > StartYear)
            {
                var diff = (year - StartYear) / 2;
                var mul = (int)Math.Pow(2, diff);
                int count = StartCount * mul;
                // if we go negative, then we wrapped (expected in year ~2044). 
                // Int32.Max is best we can do at this point
                if (count < 0) count = Int32.MaxValue;
                return count;
            }
            return StartCount;
        }
    }
}
