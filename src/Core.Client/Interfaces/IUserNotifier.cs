namespace Core.Client.Interfaces;

/// <summary>
///     Defines severity levels for user-facing notifications.
/// </summary>
public enum NotificationSeverity
{
    /// <summary>
    ///     Indicates a successful operation.
    /// </summary>
    Success,

    /// <summary>
    ///     Indicates an error or failure.
    /// </summary>
    Error
}


/// <summary>
///     Provides the ability to show notifications to the user.
/// </summary>
public interface IUserNotifier
{
    /// <summary>
    ///     Displays a notification message with the specified severity.
    /// </summary>
    /// <param name="message">
    ///     The notification text to display.
    /// </param>
    /// <param name="severity">
    ///     The severity level of the notification.
    /// </param>
    void Show(string message, NotificationSeverity severity);
}
