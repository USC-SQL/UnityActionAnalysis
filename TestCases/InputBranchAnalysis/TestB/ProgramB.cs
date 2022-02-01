using System;
using UnityEngine;

namespace TestCases.InputBranchAnalysis.TestB
{
    public class ProgramB
    {
#pragma warning disable CS0649
        float ax;
        int t;
#pragma warning restore CS0649

        public void Update()
        {
            ax = Input.GetAxis("Horizontal");
            int amount = (int)Math.Floor(ax * 100.0f);
            switch (amount)
            {
                case 1:
                    Console.WriteLine("A");
                    int q = amount + t;
                    if (q > 5)
                    {
                        Console.WriteLine("B");
                    }
                    break;
                case 2:
                    Console.WriteLine("C");
                    if (amount < 50)
                    {
                        Console.WriteLine("D");
                    }
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    Console.WriteLine("E");
                    if (t < 10)
                    {
                        Console.WriteLine("F");
                    }
                    break;
                default:
                    Console.WriteLine("G");
                    break;
            }

            if (t > 100)
            {
                Console.WriteLine("H");
            }
        }
    }
}
