using BrockAllen.MembershipReboot.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            const int StartYear = 2000;
            const int StartCount = 1000; 
            var sw = new Stopwatch();
            for (int year = 2000; true; year += 2)
            {
                var diff = (year - StartYear) / 2;
                var mul = (int)Math.Pow(2, diff);
                int count = StartCount * mul;
                // if we go negative, then we wrapped (expected in year ~2044). 
                // Int32.Max is best we can do at this point
                if (count < 0) count = Int32.MaxValue;

                sw.Reset();
                sw.Start();
                var result = Crypto.HashPassword("pass", count);
                sw.Stop();
                Console.WriteLine("year: {0}, mul:{1}, count:{2}, dur: {3}", year, mul, count, sw.ElapsedMilliseconds/1000.0);
                
                if (count == Int32.MaxValue)
                {
                    break;
                }
            }
        }
    }
}
