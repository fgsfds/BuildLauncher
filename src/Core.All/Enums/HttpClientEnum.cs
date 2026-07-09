using System.ComponentModel;

namespace Core.All.Enums;

/// <summary>
///     Identifies named HTTP clients used by the application.
/// </summary>
public enum HttpClientEnum
{
    /// <summary>
    ///     HTTP client configured for GitHub API requests.
    /// </summary>
    [Description("GitHub")]
    GitHub
}
