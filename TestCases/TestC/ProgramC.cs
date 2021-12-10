using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.TestC
{
    public class ProgramC
    {

        public static void Main(int x, uint y, long z, ulong w, int len)
        {
            if (len >= 4)
            {
                ulong[] arr = new ulong[len];
                arr[0] = (uint)x + y;
                arr[1] = (ulong)z + w;
                arr[2] = (uint)(x * y);
                arr[3] = (y + (ulong)z) / w;
                if (arr[0] + arr[1] + arr[2] + arr[3] == 200UL*(uint)arr.Length)
                {
                    Console.WriteLine("A");
                } else
                {
                    Console.WriteLine("B");
                }
            } else
            {
                throw new Exception("too small");
            }
        }

    }
}
