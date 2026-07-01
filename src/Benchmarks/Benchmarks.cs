using Addons.Providers;
using BenchmarkDotNet.Attributes;
using Core.Client.Helpers;

namespace Benchmarks;

/// <summary>
///     Performance benchmarks for parsing operations.
/// </summary>
[MemoryDiagnoser]
public class Benchmarks
{
    /// <summary>
    ///     Path to the addons.grpinfo test file.
    /// </summary>
    private static string _pathToAddonsGrpinfo;

    /// <summary>
    ///     Path to the libraryfolders.vdf test file.
    /// </summary>
    private static string _pathToLibraryFolders;

    /// <summary>
    ///     Global setup method, called once before any benchmark runs.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _pathToAddonsGrpinfo = Path.Combine(Directory.GetCurrentDirectory(), "Files", "addons.grpinfo");
        _pathToLibraryFolders = Path.Combine(Directory.GetCurrentDirectory(), "Files", "libraryfolders.vdf");
    }

    /// <summary>
    ///     Benchmarks parsing of addons.grpinfo files.
    /// </summary>
    [Benchmark]
    public void ParseAddonsGrpInfo()
    {
        GrpInfoProvider.Parse(_pathToAddonsGrpinfo, 100);
    }

    /// <summary>
    ///     Benchmarks parsing of libraryfolders.vdf files.
    /// </summary>
    [Benchmark]
    public void ParseLibraryFolders()
    {
        SteamHelper.GetLibrariesFromVdf(_pathToLibraryFolders);
    }
}
