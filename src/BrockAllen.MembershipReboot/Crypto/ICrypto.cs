/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public interface ICrypto
    {
        string Hash(string value);
        string Hash(string value, string key);
        string GenerateNumericCode(int digits);
        string GenerateSalt();
        string HashPassword(string password, int iterations);
        bool VerifyHashedPassword(string hashedPassword, string password);
        bool SlowEquals(string a, string b);
    }

    public class DefaultCrypto : ICrypto
    {
        public string Hash(string value)
        {
            return CryptoHelper.Hash(value);
        }

        public string Hash(string value, string key)
        {
            return CryptoHelper.Hash(value, key);
        }

        public string GenerateNumericCode(int digits)
        {
            return CryptoHelper.GenerateNumericCode(digits);
        }

        public string GenerateSalt()
        {
            return CryptoHelper.GenerateSalt();
        }

        public string HashPassword(string password, int iterations)
        {
            return CryptoHelper.HashPassword(password, iterations);
        }

        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            return CryptoHelper.VerifyHashedPassword(hashedPassword, password);
        }

        public bool SlowEquals(string a, string b)
        {
            return CryptoHelper.SlowEquals(a, b);
        }
    }
}
