using CommonCode.GitClasses.GettingWorkItemsBatch;

namespace Test01
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            GettingWorkItemsBatchExample.GetNewMethod().Wait();
        }
    }
}
