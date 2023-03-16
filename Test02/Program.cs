using System;
using System.Threading;
using System.Threading.Tasks;

namespace Test02
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Task<string> task = GetSum();

            string s1 = await task;
        }

        private static async Task<string> GetSum()
        {
            Task<string> taskA = GetItem("A");
            Task<string> taskB = GetItem("B");

            string a1 = await taskA;
            string b1 = await taskB;
            return a1 + b1;
        }

        private static async Task<string> GetItem(string a)
        {
            string Function()
            {
                Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {a}");
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(200);
                    Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} {a}");
                }

                return a;
            }

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem {a}");

            string s = await Task.Run(Function);

            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} GetItem end {a}");

            return s;
        }
    }
}
