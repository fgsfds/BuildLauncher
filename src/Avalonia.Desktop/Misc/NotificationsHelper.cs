using Avalonia.Controls.Notifications;
using Avalonia.Desktop.Helpers;
using Avalonia.Threading;

namespace Avalonia.Desktop.Misc;

/// <summary>
///     Helper that fixes crash when multiple notifications with the same text are shown.
/// </summary>
public static class NotificationsHelper
{
    /// <summary>
    ///     Initializes static members of the <see cref="NotificationsHelper" /> class.
    /// </summary>
    static NotificationsHelper()
    {
        NotificationManager = new(AvaloniaProperties.TopLevel)
        {
            MaxItems = 3,
            Position = NotificationPosition.TopRight,
            Margin = new(0, 50, 10, 0)
        };
    }

    /// <summary>
    ///     Gets the notification manager instance.
    /// </summary>
    public static WindowNotificationManager NotificationManager { get; }

    /// <summary>
    ///     Shows a notification.
    /// </summary>
    /// <param name="content">The notification content.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="expiration">Optional expiration time.</param>
    /// <param name="onClick">Optional click callback.</param>
    /// <param name="onClose">Optional close callback.</param>
    /// <param name="classes">Optional CSS classes.</param>
    public static void Show(
        object content,
        NotificationType type,
        TimeSpan? expiration = null,
        Action? onClick = null,
        Action? onClose = null,
        string[]? classes = null)
    {
        Dispatcher.UIThread.Post(() =>
                                     NotificationManager.Show(
                                         content,
                                         type,
                                         expiration,
                                         onClick,
                                         onClose,
                                         classes
                                         )
            );
    }
}
