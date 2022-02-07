using System;
using System.Diagnostics;

namespace Delegation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ClassA a = new ClassA("A");
            ClassA b = new ClassA("B");
            Stopwatch sv = new Stopwatch();
            sv.Start();
            for (var i = 0; i < 10000; i++)
            {
                a.Call("M1");
                b.Call("M1");
            }
            sv.Stop();
            Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            sv = new Stopwatch();
            sv.Start();
            for (var i = 0; i < 1000000; i++)
            {
                a.Call("M1");
                b.Call("M1");
            }
            sv.Stop();
            Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            sv = new Stopwatch();
            sv.Start();
            for (var i = 0; i < 1000000; i++)
            {
                a.M1("A");
                b.M1("B");
            }
            sv.Stop();
            Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);




            Console.ReadLine();
        }
    }
}
