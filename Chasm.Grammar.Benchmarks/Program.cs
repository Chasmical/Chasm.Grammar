﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Chasm.Grammar.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IConfig config = DefaultConfig.Instance;

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

            Console.ReadKey();
        }
    }
}
