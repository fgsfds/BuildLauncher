using Core.All.Enums;

namespace Core.All.Helpers;

/// <summary>
///     Provides common runtime properties.
/// </summary>
public static class CommonProperties
{
    /// <summary>
    ///     Gets the current operating system.
    /// </summary>
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
                throw new PlatformNotSupportedException("Only Windows and Linux are supported.");
            }
        }
    }
}
