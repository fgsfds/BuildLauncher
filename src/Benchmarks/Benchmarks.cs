using Addons.Providers;
using BenchmarkDotNet.Attributes;
using Core.Client.Helpers;

namespace Benchmarks;

[MemoryDiagnoser]
public sealed class Benchmarks
{
    private static string _pathToAddonsGrpinfo;
    private static string _pathToLibraryFolders;

    [GlobalSetup]
    public void Setup()
    {
        _pathToAddonsGrpinfo = Path.Combine(Directory.GetCurrentDirectory(), "Files", "addons.grpinfo");
        _pathToLibraryFolders = Path.Combine(Directory.GetCurrentDirectory(), "Files", "libraryfolders.vdf");
    }

    [Benchmark]
    public void ParseAddonsGrpInfo()
    {
        GrpInfoProvider.Parse(_pathToAddonsGrpinfo, 100);
    }

    [Benchmark]
    public void ParseLibraryFolders()
    {
        SteamHelper.GetLibratiesFromVdf(_pathToLibraryFolders);
    }
}
