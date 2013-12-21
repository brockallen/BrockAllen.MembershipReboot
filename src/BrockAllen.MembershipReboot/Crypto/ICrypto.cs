﻿/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */


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
}
