using Core.Client.Helpers;

namespace Tests.Unit.Helpers;

public static class NormalizerHelper
{
    internal static void NormalizeExpectedArgs(ref string args, ref string expected)
    {
        if (OperatingSystem.IsLinux())
        {
            args = args.Replace('\\', Path.DirectorySeparatorChar);
            expected = expected.Replace('\\', Path.DirectorySeparatorChar);
        }
    }

    internal static string NormalizePath(string path)
    {
        if (OperatingSystem.IsLinux())
        {
            return path.Replace('/', '\\');
        }

        return path;
    }
}
