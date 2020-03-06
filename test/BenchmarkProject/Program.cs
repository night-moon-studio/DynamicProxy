using BenchmarkDotNet.Running;
using System;
using System.IO;

namespace BenchmarkProject
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<OriginTest>();
            Console.ReadKey();
        }
    }
}
