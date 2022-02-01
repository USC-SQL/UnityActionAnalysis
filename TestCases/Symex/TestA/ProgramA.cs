using System;

namespace TestCases.Symex.TestA
{
    public class ProgramA
    {
        public static void Main(int x, int y)
        {
            if (x > 0)
            {
                if (y > 0)
                {
                    Console.WriteLine("A");
                } else
                {
                    Console.WriteLine("B");
                }
            }
            else
            {
                Console.WriteLine("C");
            }
        }
    }
}
