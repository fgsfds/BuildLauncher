using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Misc;
using Core.Client.Interfaces;

namespace Avalonia.Desktop.Services;

/// <summary>
///     Shows notifications to the user using the application's notification manager.
/// </summary>
public sealed class UserNotifier : IUserNotifier
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
    public void Show(string message, NotificationSeverity severity)
    {
        var type = severity switch
        {
            NotificationSeverity.Success => NotificationType.Success,
            NotificationSeverity.Error => NotificationType.Error,
            _ => NotificationType.Information
        };

        NotificationsHelper.Show(message, type);
    }
}
