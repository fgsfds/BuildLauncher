using Core.All.Enums;
using Core.All.Helpers;
using Tools.Tools;

namespace Tests.Unit;

public sealed class BaseToolTests
{
    [Fact]
    public void Mapster32_Name_ReturnsMapster32()
    {
        var tool = new Mapster32(null!);
        Assert.Equal("Mapster32", tool.Name);
    }

    [Fact]
    public void Mapster32_ToolEnum_ReturnsMapster32()
    {
        var tool = new Mapster32(null!);
        Assert.Equal(ToolEnum.Mapster32, tool.ToolEnum);
    }

    [Fact]
    public void Mapster32_CanBeInstalled_ReturnsFalse()
    {
        var tool = new Mapster32(null!);
        Assert.False((bool)tool.CanBeInstalled);
    }

    [Fact]
    public void Mapster32_InstallFolderPath_ContainsEDuke32()
    {
        var tool = new Mapster32(null!);
        Assert.EndsWith("EDuke32", (string?)tool.InstallFolderPath);
    }

    [Fact]
    public void Mapster32_IconId_IsHashOfToolEnum()
    {
        var tool = new Mapster32(null!);
        Assert.Equal(ToolEnum.Mapster32.GetUniqueHash(), tool.IconId);
    }

    [Fact]
    public void Mapster32_IsNotInstalled_WithoutVersionFile()
    {
        var tool = new Mapster32(null!);
        Assert.False((bool)tool.IsInstalled);
    }

    [Fact]
    public void XMapEdit_Name_ReturnsXMAPEDIT()
    {
        var tool = new XMapEdit(null!);
        Assert.Equal("XMAPEDIT", tool.Name);
    }

    [Fact]
    public void XMapEdit_ToolEnum_ReturnsXMapEdit()
    {
        var tool = new XMapEdit(null!);
        Assert.Equal(ToolEnum.XMapEdit, tool.ToolEnum);
    }

    [Fact]
    public void XMapEdit_CanBeLaunched_ReturnsTrue()
    {
        var tool = new XMapEdit(null!);
        Assert.True((bool)tool.CanBeLaunched);
    }

    [Fact]
    public void XMapEdit_GetStartToolArgs_ReturnsEmpty()
    {
        var tool = new XMapEdit(null!);
        Assert.Equal(string.Empty, tool.GetStartToolArgs());
    }

    [Fact]
    public void XMapEdit_IconId_IsHashOfToolEnum()
    {
        var tool = new XMapEdit(null!);
        Assert.Equal(ToolEnum.XMapEdit.GetUniqueHash(), tool.IconId);
    }

    [Fact]
    public void DOSBlood_Name_ReturnsDOSBlood()
    {
        var tool = new DOSBlood(null!);
        Assert.Equal("DOSBlood", tool.Name);
    }

    [Fact]
    public void DOSBlood_ToolEnum_ReturnsDOSBlood()
    {
        var tool = new DOSBlood(null!);
        Assert.Equal(ToolEnum.DOSBlood, tool.ToolEnum);
    }

    [Fact]
    public void DOSBlood_CanBeLaunched_ReturnsFalse()
    {
        var tool = new DOSBlood(null!);
        Assert.False((bool)tool.CanBeLaunched);
    }

    [Fact]
    public void DOSBlood_GetStartToolArgs_ThrowsNotSupported()
    {
        var tool = new DOSBlood(null!);
        Assert.Throws<NotSupportedException>(() => tool.GetStartToolArgs());
    }

    [Fact]
    public void DOSBlood_IconId_IsHashOfToolEnum()
    {
        var tool = new DOSBlood(null!);
        Assert.Equal(ToolEnum.DOSBlood.GetUniqueHash(), tool.IconId);
    }

    [Fact]
    public void XMapEdit_InstallText_NullByDefault()
    {
        var tool = new XMapEdit(null!);
        Assert.Null(tool.InstallText);
    }

    [Fact]
    public void Mapster32_InstallText_WhenNotInstalled_ContainsHint()
    {
        var tool = new Mapster32(null!);
        Assert.Contains("EDuke32", (string?)tool.InstallText);
    }

    [Fact]
    public void Exe_OnWindows_ReturnsWindowsExe()
    {
        if (CommonProperties.OSEnum is not OSEnum.Windows)
        {
            return;
        }

        var tool = new Mapster32(null!);
        Assert.Equal("mapster32.exe", tool.Exe);
    }
}
