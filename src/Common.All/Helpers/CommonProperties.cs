using Common.All.Enums;

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
                throw new ArgumentOutOfRangeException("Unsupported OS");
            }
        }
    }
}
