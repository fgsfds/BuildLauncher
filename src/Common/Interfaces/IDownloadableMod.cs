using Common.Enums;

namespace Common.Interfaces
{
    public interface IDownloadableMod
    {
        string? Author { get; set; }
        string Description { get; set; }
        string DownloadUrl { get; set; }
        long FileSize { get; set; }
        string FileSizeString { get; }
        GameEnum Game { get; set; }
        Guid Guid { get; set; }
        bool HasNewerVersion { get; set; }
        bool IsInstalled { get; set; }
        ModTypeEnum ModType { get; set; }
        string Name { get; set; }
        string Status { get; }
        string? Url { get; init; }
        float Version { get; set; }
        string VersionString { get; }

        string ToMarkdownString();
    }
}