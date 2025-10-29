using Common.All.Enums;
using Common.Client.Helpers;
using CommunityToolkit.Diagnostics;
using Games.Providers;
using Tools.Tools;

namespace Tools.Providers;

/// <summary>
/// Class that provides singleton instances of tool types
/// </summary>
public sealed class InstalledToolsProvider
{
    private readonly List<BaseTool> _tools;

    private readonly Mapster32 _mapster32;
    private readonly XMapEdit _xMapEdit;
    private readonly DOSBlood _dosBlood;


    public InstalledToolsProvider(InstalledGamesProvider gamesProvider)
    {
        if (!Directory.Exists(ClientProperties.ToolsFolderPath))
        {
            _ = Directory.CreateDirectory(ClientProperties.ToolsFolderPath);
        }

        _mapster32 = new(gamesProvider);
        _xMapEdit = new(gamesProvider);
        _dosBlood = new(gamesProvider);

        _tools = [_mapster32, _xMapEdit, _dosBlood];
    }


    /// <summary>
    /// Get list of all tools
    /// </summary>
    public IEnumerable<BaseTool> GetAllTools() => _tools;

    /// <summary>
    /// Get tool by enum
    /// </summary>
    /// <param name="toolEnum">Tool enum</param>
    public BaseTool GetTool(ToolEnum toolEnum)
    {
        return toolEnum switch
        {
            ToolEnum.Mapster32 => _mapster32,
            ToolEnum.XMapEdit => _xMapEdit,
            ToolEnum.DOSBlood => _dosBlood,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<BaseTool>()
        };
    }
}
