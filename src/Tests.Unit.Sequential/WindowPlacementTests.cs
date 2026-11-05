using Avalonia;
using Avalonia.Desktop.Helpers;

namespace Tests.Unit.Sequential;

/// <summary> Verifies persisted placements and monitor recovery. </summary>
public sealed class WindowPlacementTests
{
    /// <summary> Keeps valid bounds on a monitor left of the primary display. </summary>
    [Fact]
    public void FitToScreens_VisibleNegativePosition_PreservesPlacement()
    {
        var placement = new WindowPlacement(new PixelPoint(-1800, 40), new Size(1400, 850), true);
        var fitted = placement.FitToScreens([(new PixelRect(-1920, 0, 1920, 1080), 1), (new PixelRect(0, 0, 1920, 1080), 1)]);

        Assert.Equal(placement, fitted);
        Assert.Equal(placement, WindowPlacement.Parse(placement.ToString()));
    }

    /// <summary> Moves windows from a removed monitor to the nearest available position. </summary>
    [Fact]
    public void FitToScreens_RemovedMonitor_ClampsToNearestMonitor()
    {
        var placement = new WindowPlacement(new PixelPoint(4500, 100), new Size(1300, 800), false);
        var fitted = placement.FitToScreens([(new PixelRect(0, 0, 1920, 1040), 1), (new PixelRect(1920, 0, 1920, 1040), 1)]);

        Assert.Equal(new PixelPoint(2540, 100), fitted.Position);
        Assert.Equal(placement.Size, fitted.Size);
    }

    /// <summary> Fits oversized windows to the working area including display scaling and taskbar space. </summary>
    /// <param name="scale">The monitor scale factor.</param>
    [Theory]
    [InlineData(1)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    [InlineData(2)]
    public void FitToScreens_OversizedWindow_FitsUsableArea(double scale)
    {
        var placement = new WindowPlacement(new PixelPoint(-200, -100), new Size(3000, 2000), false);
        var fitted = placement.FitToScreens([(new PixelRect(0, 30, 1200, 700), scale)]);

        Assert.Equal(new PixelPoint(0, 30), fitted.Position);
        Assert.Equal(new Size(1200 / scale, 700 / scale), fitted.Size);
    }

    /// <summary> Corrects a partially visible window with the minimum movement. </summary>
    [Fact]
    public void FitToScreens_PartiallyOutside_ClampsBothAxes()
    {
        var placement = new WindowPlacement(new PixelPoint(900, 500), new Size(1300, 800), false);
        var fitted = placement.FitToScreens([(new PixelRect(0, 0, 1920, 1040), 1)]);

        Assert.Equal(new PixelPoint(620, 240), fitted.Position);
    }

    /// <summary> Rejects malformed or non-finite saved dimensions. </summary>
    /// <param name="value">The invalid saved placement.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("0;0;NaN;800;True")]
    [InlineData("0;0;1300;Infinity;False")]
    [InlineData("0;0;-1;800;True")]
    [InlineData("0;0;1300;0;True")]
    [InlineData("0;0;1300;800;invalid")]
    public void Parse_InvalidPlacement_ReturnsNull(string? value) => Assert.Null(WindowPlacement.Parse(value));
}
