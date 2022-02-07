using System;
using System.Diagnostics;

namespace Delegation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //ClassHost a = new ClassHost("A");
            //ClassHost b = new ClassHost("B");
            //Stopwatch sv = new Stopwatch();
            //sv.Start();
            //for (var i = 0; i < 10000; i++)
            //{
            //    a.Call("M1");
            //    b.Call("M1");
            //}
            //sv.Stop();
            //Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            //sv = new Stopwatch();
            //sv.Start();
            //for (var i = 0; i < 1000000; i++)
            //{
            //    a.Call("M1");
            //    b.Call("M1");
            //}
            //sv.Stop();
            //Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            //sv = new Stopwatch();
            //sv.Start();
            //for (var i = 0; i < 1000000; i++)
            //{
            //    a.M1("A");
            //    b.M1("B");
            //}
            //sv.Stop();
            //Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            ClassA a = new ClassA();
            ClassB b = new ClassB();
            ClassC c = new ClassC();
            ClassD d = new ClassD();

            Stopwatch sv = new Stopwatch();
            sv.Start();
            
            for (var i = 0; i < 1000000; i++)
            {
                DelegateStringSimple r = null;
                r = a.GetProc("M1");
                r("Test");
                r = a.GetProc("M2");
                r("Test");
                r = a.GetProc("M3");
                r("Test");
                r = a.GetProc("M4");
                r("Test");

                r = b.GetProc("M1");
                r("Test");
                r = b.GetProc("M2");
                r("Test");
                r = b.GetProc("M3");
                r("Test");
                r = b.GetProc("M4");
                r("Test");

                r = c.GetProc("M1");
                r("Test");
                r = c.GetProc("M2");
                r("Test");
                r = c.GetProc("M3");
                r("Test");
                r = c.GetProc("M4");
                r("Test");

                r = d.GetProc("M1");
                r("Test");
                r = d.GetProc("M2");
                r("Test");
                r = d.GetProc("M3");
                r("Test");
                r = d.GetProc("M4");
                r("Test");
            }
            sv.Stop();
            Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            sv = new Stopwatch();
            sv.Start();
            for (var i = 0; i < 1000000; i++)
            {
                a.M1("Test");
                a.M2("Test");
                a.M3("Test");
                a.M4("Test");

                b.M1("Test");
                b.M2("Test");
                b.M3("Test");
                b.M4("Test");

                c.M1("Test");
                c.M2("Test");
                c.M3("Test");
                c.M4("Test");

                d.M1("Test");
                d.M2("Test");
                d.M3("Test");
                d.M4("Test");
            }
            sv.Stop();
            Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            sv = new Stopwatch();
            sv.Start();
            DelegateStringA rA = null;
            DelegateStringB rB = null;
            DelegateStringC rC = null;
            DelegateStringD rD = null;
            for (var i = 0; i < 1000000; i++)
            {
                rA = a.GetProc2("M1");
                rA(a, "Test");
                rA = a.GetProc2("M2");
                rA(a, "Test");
                rA = a.GetProc2("M3");
                rA(a, "Test");
                rA = a.GetProc2("M4");
                rA(a, "Test");

                rB = b.GetProc2("M1");
                rB(b, "Test");
                rB = b.GetProc2("M2");
                rB(b, "Test");
                rB = b.GetProc2("M3");
                rB(b, "Test");
                rB = b.GetProc2("M4");
                rB(b, "Test");

                rC = c.GetProc2("M1");
                rC(c, "Test");
                rC = c.GetProc2("M2");
                rC(c, "Test");
                rC = c.GetProc2("M3");
                rC(c, "Test");
                rC = c.GetProc2("M4");
                rC(c, "Test");

                rD = d.GetProc2("M1");
                rD(d, "Test");
                rD = d.GetProc2("M2");
                rD(d, "Test");
                rD = d.GetProc2("M3");
                rD(d, "Test");
                rD = d.GetProc2("M4");
                rD(d, "Test");
            }
            sv.Stop();
            Console.WriteLine("Ellapsed {0}", sv.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }
}
