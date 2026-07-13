using Core.All.Enums;
using Games.Games;

namespace Tests.Unit;

public sealed class GameSpecificTests
{
    [Fact]
    public void DukeGame_GameEnum_ReturnsDuke3D()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.Equal(GameEnum.Duke3D, game.GameEnum);
    }

    [Fact]
    public void DukeGame_FullName_ReturnsDukeNukem3D()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.Equal("Duke Nukem 3D", game.FullName);
    }

    [Fact]
    public void DukeGame_ShortName_ReturnsDuke3D()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.Equal("Duke3D", game.ShortName);
    }

    [Fact]
    public void DukeGame_RequiredFiles_ReturnsDuke3dGrp()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.Contains("DUKE3D.GRP", game.RequiredFiles);
    }

    [Fact]
    public void DukeGame_Skills_IsNotNull()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.NotNull(game.Skills);
        Assert.True(game.AreSkillsAvailable);
    }

    [Fact]
    public void DukeGame_IsDuke64Installed_NullPath_ReturnsFalse()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.False(game.IsDuke64Installed);
    }

    [Fact]
    public void DukeGame_IsDukeZHInstalled_NullPath_ReturnsFalse()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.False(game.IsDukeZHInstalled);
    }

    [Fact]
    public void DukeGame_IsWorldTourInstalled_NullPath_ReturnsFalse()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.False(game.IsWorldTourInstalled);
    }

    [Fact]
    public void DukeGame_CampaignsFolderPath_ContainsDuke3D()
    {
        var game = new DukeGame { Duke64RomPath = null, DukeZHRomPath = null, DukeWTInstallPath = null };
        Assert.Contains("Duke3D", game.CampaignsFolderPath);
    }

    [Fact]
    public void BloodGame_GameEnum_ReturnsBlood()
    {
        var game = new BloodGame();
        Assert.Equal(GameEnum.Blood, game.GameEnum);
    }

    [Fact]
    public void BloodGame_FullName_ReturnsBlood()
    {
        var game = new BloodGame();
        Assert.Equal("Blood", game.FullName);
    }

    [Fact]
    public void BloodGame_ShortName_ReturnsBlood()
    {
        var game = new BloodGame();
        Assert.Equal("Blood", game.ShortName);
    }

    [Fact]
    public void BloodGame_RequiredFiles_ContainsBloodIni()
    {
        var game = new BloodGame();
        Assert.Contains("BLOOD.INI", game.RequiredFiles);
    }

    [Fact]
    public void BloodGame_Skills_IsNotNull()
    {
        var game = new BloodGame();
        Assert.NotNull(game.Skills);
        Assert.True(game.AreSkillsAvailable);
    }

    [Fact]
    public void BloodGame_IsCrypticPassageInstalled_NullGameFolder_ReturnsFalse()
    {
        var game = new BloodGame();
        Assert.False(game.IsCrypticPassageInstalled);
    }

    [Fact]
    public void SlaveGame_Properties()
    {
        var game = new SlaveGame();
        Assert.Equal(GameEnum.Slave, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void NamGame_Properties()
    {
        var game = new NamGame();
        Assert.Equal(GameEnum.NAM, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void RedneckGame_Properties()
    {
        var game = new RedneckGame { AgainInstallPath = null };
        Assert.Equal(GameEnum.Redneck, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void WangGame_Properties()
    {
        var game = new WangGame();
        Assert.Equal(GameEnum.Wang, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void WW2GIGame_Properties()
    {
        var game = new WW2GIGame();
        Assert.Equal(GameEnum.WW2GI, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void FuryGame_Properties()
    {
        var game = new FuryGame();
        Assert.Equal(GameEnum.Fury, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void WitchavenGame_Properties()
    {
        var game = new WitchavenGame();
        Assert.Equal(GameEnum.Witchaven, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }

    [Fact]
    public void TekWarGame_Properties()
    {
        var game = new TekWarGame();
        Assert.Equal(GameEnum.TekWar, game.GameEnum);
        Assert.NotEmpty(game.FullName);
        Assert.NotEmpty(game.ShortName);
        Assert.NotEmpty(game.RequiredFiles);
    }
}
