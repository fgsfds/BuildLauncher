using Addons.Addons;
using Core.All.Enums;
using Core.Client.Helpers;
using Games.Games;

namespace Tests.Unit.Sync;

[Collection("Sync")]
public sealed class SaveFilesTests : IDisposable
{
    private readonly DukeGame _game;
    private readonly DukeCampaign _camp;
    private readonly string _gameDir;
    private readonly List<string> _tempDirs = [];

    public SaveFilesTests()
    {
        _gameDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_gameDir);

        _game = new DukeGame
        {
            Duke64RomPath = null,
            DukeZHRomPath = null,
            DukeWTInstallPath = null,
            GameInstallFolder = _gameDir,
            AddonsPaths = [],
        };

        _camp = new DukeCampaign
        {
            AddonId = new("save-test", null),
            Type = AddonTypeEnum.Official,
            Title = "Save Test",
            SupportedGame = new(GameEnum.Duke3D, null, null),
            FileInfo = null,
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null,
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_gameDir))
        {
            Directory.Delete(_gameDir, true);
        }

        foreach (var dir in _tempDirs)
        {
            if (Directory.Exists(dir))
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }
    }

    private void RegisterTempDir(string path)
    {
        _tempDirs.Add(path);
    }

    [Fact]
    public void Base_MoveSaveFilesToStorage_NoSaves_DoesNotThrow()
    {
        var port = new BasePortTestProxy();
        port.CallMoveSaveFilesToStorage(_game, _camp);
    }

    [Fact]
    public void Base_MoveSaveFilesFromStorage_NoFolder_DoesNotThrow()
    {
        var port = new BasePortTestProxy();
        port.CallMoveSaveFilesFromStorage(_game, _camp);
    }

    [Fact]
    public void Base_MoveSaveFilesToStorage_MovesSaves()
    {
        var port = new BasePortTestProxy();
        var saveFolder = port.CallGetPathToAddonSavedGamesFolder(_game.ShortName, _camp.AddonId.Id);
        CleanupParentDirs(saveFolder);

        var saveFile1 = Path.Combine(_gameDir, "savegame.sav");
        var saveFile2 = Path.Combine(_gameDir, "quick.esv");
        var nonSaveFile = Path.Combine(_gameDir, "readme.txt");
        File.WriteAllText(saveFile1, "save1");
        File.WriteAllText(saveFile2, "save2");
        File.WriteAllText(nonSaveFile, "don't move me");

        port.CallMoveSaveFilesToStorage(_game, _camp);

        Assert.False(File.Exists(saveFile1));
        Assert.False(File.Exists(saveFile2));
        Assert.True(File.Exists(nonSaveFile));
        Assert.True(Directory.Exists(saveFolder));

        var savedFiles = Directory.GetFiles(saveFolder);
        Assert.Equal(2, savedFiles.Length);
        Assert.Contains(savedFiles, f => f.EndsWith("savegame.sav"));
        Assert.Contains(savedFiles, f => f.EndsWith("quick.esv"));
    }

    [Fact]
    public void Base_MoveSaveFilesFromStorage_RestoresSaves()
    {
        var port = new BasePortTestProxy();
        var saveFolder = port.CallGetPathToAddonSavedGamesFolder(_game.ShortName, _camp.AddonId.Id);
        CleanupParentDirs(saveFolder);

        var saveFile = Path.Combine(_gameDir, "savegame.sav");
        File.WriteAllText(saveFile, "save data");

        port.CallMoveSaveFilesToStorage(_game, _camp);
        Assert.False(File.Exists(saveFile));
        Assert.True(Directory.Exists(saveFolder));

        port.CallMoveSaveFilesFromStorage(_game, _camp);

        Assert.True(File.Exists(saveFile), "Save file should be restored");
        Assert.Equal("save data", File.ReadAllText(saveFile));
    }

    [Fact]
    public void EDuke32_MoveSaveFilesFromStorage_NullFileInfo_UsesInstallFolder()
    {
        var port = new EDuke32TestProxy();
        var saveFolder = port.CallGetPathToAddonSavedGamesFolder(_game.ShortName, _camp.AddonId.Id);
        CleanupParentDirs(saveFolder);

        // Create the install directory (EDuke32's PortsFolder/EDuke32)
        var installDir = Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");
        Directory.CreateDirectory(installDir);
        RegisterTempDir(installDir);

        // EDuke32's MoveSaveFilesFromStorage copies FROM saveFolder TO installDir
        Directory.CreateDirectory(saveFolder);
        var saveFile = Path.Combine(saveFolder, "savegame.sav");
        File.WriteAllText(saveFile, "save data");

        port.CallMoveSaveFilesFromStorage(_game, _camp);

        // Save should be in install directory
        Assert.True(File.Exists(Path.Combine(installDir, "savegame.sav")), "Save should be restored to install dir");
        Assert.False(File.Exists(saveFile), "Save should be removed from save folder");
    }

    [Fact]
    public void EDuke32_MoveSaveFilesToStorage_NullFileInfo_UsesInstallFolder()
    {
        var port = new EDuke32TestProxy();
        var saveFolder = port.CallGetPathToAddonSavedGamesFolder(_game.ShortName, _camp.AddonId.Id);
        CleanupParentDirs(saveFolder);

        // Create the install directory (EDuke32's PortsFolder/EDuke32)
        var installDir = Path.Combine(ClientProperties.PortsFolderPath, "EDuke32");
        Directory.CreateDirectory(installDir);

        // EDuke32's MoveSaveFilesToStorage copies FROM installDir TO saveFolder
        var saveFile = Path.Combine(installDir, "savegame.sav");
        File.WriteAllText(saveFile, "save data");

        port.CallMoveSaveFilesToStorage(_game, _camp);

        // Save should be in save folder
        Assert.True(Directory.Exists(saveFolder), "Save folder should exist");
        Assert.True(File.Exists(Path.Combine(saveFolder, "savegame.sav")), "Save should be backed up to save folder");
        Assert.False(File.Exists(saveFile), "Save should be removed from install dir");
    }

    [Fact]
    public void EDuke32_MoveSaveFilesFromStorage_FolderFileInfo_RestoresToAddonFolder()
    {
        var addonDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(addonDir);
        RegisterTempDir(addonDir);

        var camp = new DukeCampaign
        {
            AddonId = new("folder-camp", null),
            Type = AddonTypeEnum.TC,
            Title = "Folder Camp",
            SupportedGame = new(GameEnum.Duke3D, null, null),
            FileInfo = new AddonFilePathWrapper(addonDir, "addon.json"),
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null,
        };

        var port = new EDuke32TestProxy();
        var saveFolder = port.CallGetPathToAddonSavedGamesFolder(_game.ShortName, camp.AddonId.Id);
        CleanupParentDirs(saveFolder);

        // FromStorage (RESTORE): saves from saveFolder → addon folder
        Directory.CreateDirectory(saveFolder);
        var saveFile = Path.Combine(saveFolder, "savegame.sav");
        File.WriteAllText(saveFile, "save data");

        port.CallMoveSaveFilesFromStorage(_game, camp);

        Assert.True(File.Exists(Path.Combine(addonDir, "savegame.sav")), "Save should be restored to addon folder");
        Assert.False(File.Exists(saveFile), "Save should be removed from save folder");
    }

    [Fact]
    public void EDuke32_MoveSaveFilesToStorage_FolderFileInfo_BacksUpFromAddonFolder()
    {
        var addonDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(addonDir);
        RegisterTempDir(addonDir);

        var camp = new DukeCampaign
        {
            AddonId = new("folder-camp", null),
            Type = AddonTypeEnum.TC,
            Title = "Folder Camp",
            SupportedGame = new(GameEnum.Duke3D, null, null),
            FileInfo = new AddonFilePathWrapper(addonDir, "addon.json"),
            GridImageHash = null,
            PreviewImageHash = null,
            Description = null,
            Author = null,
            ReleaseDate = null,
            MainDef = null,
            AdditionalDefs = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null,
            StartMap = null,
            DependentAddons = null,
            IncompatibleAddons = null,
            RequiredFeatures = null,
            Executables = null,
            Options = null,
        };

        var port = new EDuke32TestProxy();
        var saveFolder = port.CallGetPathToAddonSavedGamesFolder(_game.ShortName, camp.AddonId.Id);
        CleanupParentDirs(saveFolder);

        // ToStorage (BACKUP): saves from addon folder → saveFolder
        var saveFile = Path.Combine(addonDir, "savegame.sav");
        File.WriteAllText(saveFile, "save data");

        port.CallMoveSaveFilesToStorage(_game, camp);

        Assert.True(File.Exists(Path.Combine(saveFolder, "savegame.sav")), "Save should be backed up to save folder");
        Assert.False(File.Exists(saveFile), "Save should be removed from addon folder");
    }

    private void CleanupParentDirs(string path)
    {
        var dir = Path.GetDirectoryName(path);
        while (dir is not null && Directory.Exists(dir))
        {
            try
            {
                if (Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    break;
                }
                Directory.Delete(dir);
                dir = Path.GetDirectoryName(dir);
            }
            catch
            {
                break;
            }
        }
    }
}
