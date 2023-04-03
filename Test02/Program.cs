using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test02
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            //await GetNewMethod();

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Main Start");
            Task<int> task = Level1();

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Main Midle");
            int i = await task;

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Main End");

            Console.WriteLine($"i: {i}");
        }

        private static async Task<int> Level1()
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level1 Start");

            Task<int> task = Level2();

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level1 Midle");
            
            int i = await task;
            
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level1 End");
            return i;
        }

        private static async Task<int> Level2()
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level2 Start");

            int i = await Level3();
            
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level2 End");
            return i;
        }

        private static async Task<int> Level3()
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level3 Start");

            int i = await Task.Run(() =>
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level3 Task");
                Thread.Sleep(100);
                return 42;
            });
            
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Level3 End");

            return i;
        }

        private static async Task GetNewMethod()
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Main Start");

            //Task<string> task = GetSum();

            //Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Main Loop");

            string s1 = await Task.Run(GetSum);

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} Main End: {s1}");
        }

        private static async Task<string> GetSum()
        {
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetSum Start");

            Task<string> taskA = GetItem("A");
            Task<string> taskB = GetItem("B");

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(100);
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetSum Loop 1");
            }

            string a1 = await taskA;

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(100);
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetSum Loop 2");
            }
            
            string b1 = await taskB;

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetSum End: {a1}, {b1}");
            
            return a1 + b1;
        }

        private static async Task<string> GetItem(string a)
        {
            string Function()
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem:Function Start {a}");
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(200);
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem:Function Loop {a}");
                }

                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem:Function End {a}");
                return a;
            }

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem Start {a}");

            string s = await Task.Run(Function);

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem End {a}");

            return s;
        }
    }
}
