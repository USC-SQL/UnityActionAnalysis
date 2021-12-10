using System;
using System.Collections.Generic;
using System.Text;

namespace TestCases.TestD
{
    public struct PersonIdentifier
    {
        public string name;
        public int id;
    }

    public class Person
    {
        private PersonIdentifier ident;

        public PersonIdentifier Identifier { get => ident; }

        public int FavoriteColor { get; set;  }

        public Person(string name, int id)
        {
            ident = new PersonIdentifier();
            ident.name = name;
            ident.id = id;
            FavoriteColor = 1;
        }

        public int Calc()
        {
            int y = ident.id;
            for (int i = 0; i < 5; ++i)
            {
                y += i * y;
            }
            return y;
        }
    }

    public class ProgramD
    {
        private int P1FavoriteColor { get; set; }
        private int P2FavoriteColor { get; set; }

        public void Main(string name1, int id1, string name2, int id2)
        {
            Person p1 = new Person(name1, id1);
            Person p2 = new Person(name2, id2);
            p1.FavoriteColor = P1FavoriteColor;
            p2.FavoriteColor = P2FavoriteColor;
            if (p1.Identifier.id > p2.Identifier.id)
            {
                if (id1 == p2.Calc())
                {
                    if (p1.FavoriteColor == p2.FavoriteColor + 1)
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
            } else
            {
                Console.WriteLine("D");
            }
        }
    }
}
