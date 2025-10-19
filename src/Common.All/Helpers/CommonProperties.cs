using Common.All.Enums;
using CommunityToolkit.Diagnostics;

namespace Common.All.Helpers;

public static class CommonProperties
{
    public static OSEnum OSEnum
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
                return OSEnum.Windows;
            }
            else if (OperatingSystem.IsLinux())
            {
                return OSEnum.Linux;
            }
            else
            {
                return ThrowHelper.ThrowArgumentOutOfRangeException<OSEnum>("Unsupported OS");
            }
        }
    }
}
