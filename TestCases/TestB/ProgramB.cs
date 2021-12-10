using System;

namespace TestCases.TestB
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class ProgramB
    {
        public static float VectorSum(Vector3 v)
        {
            return v.x + v.y + v.z;
        }

        public static Vector3 MakeVector(int x, int y, int z)
        {
            return new Vector3(x, y, z);
        }

        public static void Main(int x, int y, int z)
        {
            Vector3 v = MakeVector(x, y, z);
            if (VectorSum(v) == 10)
            {
                Console.WriteLine("A");
            } else if (VectorSum(v) == 15)
            {
                throw new Exception("exception");
            } else
            {
                Console.WriteLine("B");
            }
        }
    }
}
