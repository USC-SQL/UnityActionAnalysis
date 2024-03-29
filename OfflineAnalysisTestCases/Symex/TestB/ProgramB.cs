﻿using System;

namespace OfflineAnalysisTestCases.Symex.TestB
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
            float sum = VectorSum(v);
            if (sum <= 10000.0f)
            {
                Console.WriteLine("A");
            } else
            {
                throw new Exception("exception");
            }
        }
    }
}
