using BenchmarkDotNet.Running;

namespace Benchmarks;

public static class Program
{
    private static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
