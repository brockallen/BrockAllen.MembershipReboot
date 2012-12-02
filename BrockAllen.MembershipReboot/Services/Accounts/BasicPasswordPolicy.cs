using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrockAllen.MembershipReboot
{
    public class BasicPasswordPolicy : IPasswordPolicy
    {
        public uint MinLength { get; set; }
        public uint UpperAlphas { get; set; }
        public uint LowerAlphas { get; set; }
        public uint Numerics { get; set; }
        public uint NonAlphaNumerics { get; set; }

        public string PolicyMessage
        {
            get 
            {
                var sb = new StringBuilder();
                if (UpperAlphas > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.AppendFormat("{0} uppercase", UpperAlphas);
                }
                if (LowerAlphas > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.AppendFormat("{0} lowercase", LowerAlphas);
                }
                if (Numerics > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.AppendFormat("{0} numeric", Numerics);
                }
                if (NonAlphaNumerics > 0)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.AppendFormat("{0} non-alphanumeric", NonAlphaNumerics);
                }

                if (sb.Length > 0)
                {
                    sb.Insert(0, "Password must contain at least: ");
                    sb.Append(" characters. ");
                }

                var min = Math.Max(MinLength, UpperAlphas + LowerAlphas + Numerics + NonAlphaNumerics);
                if (min > 0)
                {
                    sb.Insert(0, String.Format("Password must be at least {0} characters long. ", min));
                }

                return sb.ToString();
            }
        }

        public bool ValidatePassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password)) return false;

            int length = password.Length;
            int upper = GetUpperAlphas(password);
            int lower = GetLowerAlphas(password);
            int numerics = GetNumerics(password);
            int non = GetNonAlphaNumerics(password);

            return
                length >= MinLength &&
                upper >= UpperAlphas &&
                lower >= LowerAlphas &&
                numerics >= Numerics &&
                non >= NonAlphaNumerics;
        }

        int GetUpperAlphas(string password)
        {
            return password.Count(x => 'A' <= x && x <= 'Z');
        }
        int GetLowerAlphas(string password)
        {
            return password.Count(x => 'a' <= x && x <= 'z');
        }
        int GetNumerics(string password)
        {
            return password.Count(x => '0' <= x && x <= '9');
        }
        int GetNonAlphaNumerics(string password)
        {
            return password.Length - GetUpperAlphas(password) - GetLowerAlphas(password) - GetNumerics(password);
        }
    }
}
