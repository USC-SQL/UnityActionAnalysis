using System;

namespace TestCases.Symex.TestK
{
    public class Base
    {
    }

    public class A : Base
    {
    }

    public class B : Base
    {
    }

    public class ProgramK
    {
        public void Main()
        {
            Base x = new A();
            Base y = new B();
            if (x is A)
            {
                if (y is B)
                {
                    Console.WriteLine("A");
                } else
                {
                    Console.WriteLine("B");
                }
            } else
            {
                Console.WriteLine("C");
            }
        }
    }
}
