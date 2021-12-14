using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.TestI
{
    struct Record
    {
        public int x;

#pragma warning disable CS0824 
        public extern Record(uint id);
#pragma warning restore CS0824
    }

    public class ProgramI
    {

        public void Main()
        {
            Record r = new Record(2);
            Record r2 = new Record(3);
            switch (r.x)
            {
                case 1:
                    switch (r2.x)
                    {
                        case 1:
                            Console.WriteLine("A");
                            break;
                        case 2:
                            Console.WriteLine("B");
                            break;
                        case 3:
                            Console.WriteLine("C");
                            break;
                        case 4:
                            Console.WriteLine("D");
                            break;
                        case 5:
                            Console.WriteLine("E");
                            break;
                        case 6:
                            Console.WriteLine("F");
                            break;
                        case 7:
                            Console.WriteLine("G");
                            break;
                        case 8:
                            Console.WriteLine("H");
                            break;
                        default:
                            Console.WriteLine("H2");
                            break;
                    }
                    break;
                case 2:
                    Console.WriteLine("I");
                    break;
                case 3:
                    Console.WriteLine("J");
                    break;
                case 4:
                    Console.WriteLine("K");
                    break;
                case 5:
                    Console.WriteLine("L");
                    break;
                case 6:
                    Console.WriteLine("M");
                    break;
                case 7:
                    Console.WriteLine("N");
                    break;
                case 8:
                    Console.WriteLine("O");
                    break;
                case 9:
                    Console.WriteLine("P");
                    break;
                case 10:
                    Console.WriteLine("Q");
                    break;
                case 11:
                    Console.WriteLine("R");
                    break;
                case 12:
                    Console.WriteLine("S");
                    break;
                case 13:
                    Console.WriteLine("T");
                    break;
                case 14:
                    Console.WriteLine("U");
                    break;
                case 15:
                    Console.WriteLine("V");
                    break;
                case 16:
                    Console.WriteLine("W");
                    break;
                case 17:
                    Console.WriteLine("X");
                    break;
                case 18:
                    Console.WriteLine("Y");
                    break;
                case 19:
                    Console.WriteLine("Z");
                    break;
                case 20:
                    Console.WriteLine("AA");
                    break;
                case 21:
                    Console.WriteLine("AB");
                    break;
                default:
                    Console.WriteLine("AC");
                    break;
            }
        }

    }
}
