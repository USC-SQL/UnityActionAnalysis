using System;
using System.Collections.Generic;
using System.Text;

namespace OfflineAnalysisTestCases.Symex.TestJ
{
    public class ProgramJ
    {
#pragma warning disable CS0626
        public static extern int GetAxis(string axisName);
#pragma warning restore CS0626


        public void Main()
        {
            string horizontalAxis = "Horizontal";
            string verticalAxis = "Vertical";
            if (GetAxis(horizontalAxis) > 0)
            {
                if (GetAxis(verticalAxis) > 0)
                {
                    Console.WriteLine("North East");
                } 
                else if (GetAxis(verticalAxis) < 0)
                {
                    Console.WriteLine("South East");
                } else
                {
                    Console.WriteLine("East");
                }
            } 
            else if (GetAxis(horizontalAxis) < 0)
            {
                if (GetAxis(verticalAxis) > 0)
                {
                    Console.WriteLine("North West");
                }
                else if (GetAxis(verticalAxis) < 0)
                {
                    Console.WriteLine("South West");
                }
                else
                {
                    Console.WriteLine("West");
                }
            } 
            else
            {
                Console.WriteLine("Origin");
            }
        }
    }
}
