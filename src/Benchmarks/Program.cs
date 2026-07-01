using BenchmarkDotNet.Running;

namespace Benchmarks;

/// <summary>
///     Entry point for running benchmarks.
/// </summary>
public static class Program
{
    /// <summary>
    ///     Entry point for running benchmarks.
    /// </summary>
    private static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
