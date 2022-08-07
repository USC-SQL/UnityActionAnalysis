using System;
using UnityEngine;

namespace OfflineAnalysisTestCases.InputBranchAnalysis.TestA
{
    public class ProgramA
    {
        public int Sum(int x, int y)
        {
            return x + y;
        }

        public void Extra()
        {
            Console.WriteLine("extra");
        }

        private float AxisInverted(string axisName)
        {
            float axis = Input.GetAxis(axisName);
            return -axis;
        }

        private void f()
        {
            float x = Input.GetAxis("Horizontal");
            float y = x + 1.0f;
            if (y > 0.0f)
            {
                Console.WriteLine("A");
                if (Input.GetButtonDown("Fire"))
                {
                    Console.WriteLine("B");
                }
            } else
            {
                Console.WriteLine("C");
            }
        }

#pragma warning disable CS0649
        int x;
        int y;
#pragma warning restore CS0649

        public void Update()
        {
            bool b = Input.GetButtonDown("Fire");
            bool b2 = !b;
            if (b2)
            {
                Console.WriteLine("A");
            }

            Console.WriteLine("B");

            float vert = AxisInverted("Vertical");
            if (vert > 0.0f)
            {
                Console.WriteLine("C");
            }

            f();

            Console.WriteLine("D");

            int z = Sum(x, y);
            if (z > 0)
            {
                Console.WriteLine("E");
            }
        }

    }
}
