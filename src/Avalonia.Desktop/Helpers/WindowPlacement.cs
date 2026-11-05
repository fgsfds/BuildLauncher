using System.Globalization;

namespace Avalonia.Desktop.Helpers;

/// <summary> Stores the normal window bounds and whether the window was maximized. </summary>
/// <param name="Position">The desktop position in physical pixels.</param>
/// <param name="Size">The normal window size in device-independent units.</param>
/// <param name="IsMaximized">Whether to restore the maximized state.</param>
public sealed record WindowPlacement(PixelPoint Position, Size Size, bool IsMaximized)
{
    /// <summary> Reads a saved placement, returning null for missing or invalid settings. </summary>
    /// <param name="value">The persisted placement.</param>
    /// <returns>The valid placement, or null.</returns>
    public static WindowPlacement? Parse(string? value)
    {
        var parts = value?.Split(';');

        if (parts is not { Length: 5 }
         || !int.TryParse(parts[0], CultureInfo.InvariantCulture, out var x)
         || !int.TryParse(parts[1], CultureInfo.InvariantCulture, out var y)
         || !double.TryParse(parts[2], CultureInfo.InvariantCulture, out var width)
         || !double.TryParse(parts[3], CultureInfo.InvariantCulture, out var height)
         || !bool.TryParse(parts[4], out var maximized)
         || !double.IsFinite(width) || width <= 0 || width > int.MaxValue
         || !double.IsFinite(height) || height <= 0 || height > int.MaxValue)
        {
            return null;
        }

        return new WindowPlacement(new PixelPoint(x, y), new Size(width, height), maximized);
    }

    /// <summary> Fits the window to the monitor that requires the least movement of its saved position. </summary>
    /// <param name="screens">The usable monitor bounds and their display scales.</param>
    /// <returns>The corrected placement, or the original placement if no monitors are available.</returns>
    public WindowPlacement FitToScreens(IReadOnlyList<(PixelRect WorkingArea, double Scaling)> screens)
    {
        var closest = this;
        var closestDistance = double.PositiveInfinity;

        foreach (var (area, scale) in screens)
        {
            if (area.Width <= 0 || area.Height <= 0 || !double.IsFinite(scale) || scale <= 0)
            {
                continue;
            }

            var availableWidth = area.Width / scale;
            var availableHeight = area.Height / scale;
            var width = Math.Clamp(Size.Width, Math.Min(1300, availableWidth), availableWidth);
            var height = Math.Clamp(Size.Height, Math.Min(800, availableHeight), availableHeight);
            var x = (int)Math.Clamp((double)Position.X, area.X, (double)area.Right - Math.Min(area.Width, Math.Ceiling(width * scale)));
            var y = (int)Math.Clamp((double)Position.Y, area.Y, (double)area.Bottom - Math.Min(area.Height, Math.Ceiling(height * scale)));
            var dx = (double)x - Position.X;
            var dy = (double)y - Position.Y;
            var distance = (dx * dx) + (dy * dy);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = new WindowPlacement(new PixelPoint(x, y), new Size(width, height), IsMaximized);
            }
        }

        return closest;
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Position.X};{Position.Y};{Size.Width:R};{Size.Height:R};{IsMaximized}");
}
