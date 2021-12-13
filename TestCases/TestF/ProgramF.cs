using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.TestF
{
    public class Obj
    {
        public int x;
    }

    public class ProgramF
    {
        private double d1;
        private long l1;

        public void Main(int len, int xval)
        {
            d1 = 0.1;
            l1 = 200L;
            Obj obj1 = new Obj();
            obj1.x = (int)(l1 * d1);
            Obj obj2;
            if (xval > 0)
            {
                obj2 = new Obj();
                obj2.x = xval;
            } else
            {
                obj2 = null;
            }

            long[] arr = new long[10] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            long[] arr2 = new long[len];
            if (arr2.Length == arr.Length)
            {
                if (obj2 != null)
                {
                    if (obj1 != obj2)
                    {
                        if (obj2.x == obj1.x)
                        {
                            Console.WriteLine("E");
                        } else
                        {
                            Console.WriteLine("D");
                        }
                    } else
                    {
                        Console.WriteLine("C");
                    }
                } else
                {
                    Console.WriteLine("B");
                }
            } else
            {
                Console.WriteLine("A");
            }
        }
    }
}
