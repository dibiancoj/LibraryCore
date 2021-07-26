using BenchmarkDotNet.Running;
using LibraryCore.Performance.Tests.Tests;
using System;

namespace LibraryCore.Performance.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = BenchmarkRunner.Run<UnionTypePerfTest>();

            Console.WriteLine("Performance Test Complete. Press Any Key To Exit.");
            Console.ReadKey();
        }
    }
}
