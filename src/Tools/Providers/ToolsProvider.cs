using Common.All.Enums;
using Common.Client.Helpers;
using Tools.Tools;

namespace Tools.Providers;

/// <summary>
/// Class that provides singleton instances of tool types
/// </summary>
public sealed class ToolsProvider
{
    private readonly Dictionary<ToolEnum, BaseTool> _tools = [];

    public ToolsProvider(IEnumerable<BaseTool> tools)
    {
        if (!Directory.Exists(ClientProperties.ToolsFolderPath))
        {
            _ = Directory.CreateDirectory(ClientProperties.ToolsFolderPath);
        }

        foreach (var tool in tools)
        {
            _tools.Add(tool.ToolEnum, tool);
        }
    }


    /// <summary>
    /// Get list of all tools
    /// </summary>
    public IReadOnlyList<BaseTool> GetAllTools() => [.. _tools.Values];

    /// <summary>
    /// Get tool by enum
    /// </summary>
    /// <param name="toolEnum">Tool enum</param>
    public BaseTool GetTool(ToolEnum toolEnum) =>
        _tools.TryGetValue(toolEnum, out var tool) ? tool : throw new ArgumentException();
}
