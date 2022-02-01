using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.Symex.TestE
{
    public class Record
    {
        public int recordId;
    }

    public class ProgramE
    {
        private Dictionary<int, Record> dict = new Dictionary<int, Record>();

        public void Main(int key, int y)
        {
            Record rec = dict[key];
            if (rec.recordId == y)
            {
                Console.WriteLine("A");
            } else
            {
                Console.WriteLine("B");
            }
        }
    }
}
