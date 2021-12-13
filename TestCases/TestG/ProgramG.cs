using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.TestG
{
    public class Vector2
    {
        private float x;
        private float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float MagnitudeSquared()
        {
            return x * x + y * y;
        }
    }

    public struct Record
    {
        public ulong id;
        public string name;
        public Vector2 position;

        public Record(ulong id, string name, Vector2 position)
        {
            this.id = id;
            this.name = name;
            this.position = position;
        }
    }

    public static class GlobalState
    {
        public static Record firstRecord;
        public static Record secondRecord;
        public static Record thirdRecord;
    }

    public class ProgramG
    {
        private static extern Record FetchRecordFromDB(int id);

        private static bool CompareRecords(Record rec1, Record rec2)
        {
            return rec1.id < rec2.id;
        }

        public void Main()
        {
            Record rec1 = FetchRecordFromDB(1);
            Record rec2 = FetchRecordFromDB(2);

            GlobalState.firstRecord = rec1;
            GlobalState.secondRecord = rec2;

            if (CompareRecords(rec1, rec2))
            {
                float firstMagn = GlobalState.firstRecord.position.MagnitudeSquared();
                float secondThirdMagn = GlobalState.secondRecord.position.MagnitudeSquared() + GlobalState.thirdRecord.position.MagnitudeSquared();
                if (firstMagn > secondThirdMagn)
                {
                    Console.WriteLine("A");
                }
                else
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
