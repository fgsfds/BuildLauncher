using Core.Client.Helpers;

namespace Tests.Unit.Helpers;

public static class NormalizerHelper
{
    /// <summary>
    ///     Normalizes path separators in both <paramref name="args" /> and <paramref name="expected" />
    ///     to the platform native separator on Linux, so that command-line argument tests pass cross-platform.
    /// </summary>
    internal static void NormalizeExpectedArgs(ref string args, ref string expected)
    {
        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }
    }

    /// <summary>
    ///     Converts forward slashes to backslashes on Linux so that paths returned
    ///     by <see cref="AddonFilePathWrapper" /> (which uses native separators) can
    ///     be compared against hardcoded Windows-style expected values in tests.
    /// </summary>
    internal static string NormalizePath(string path)
    {
        if (OperatingSystem.IsLinux())
        {
            return path.Replace('/', '\\');
        }

        return path;
    }
}
