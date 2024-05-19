using Common.Enums;

namespace Common.Interfaces
{
    public interface IDownloadableAddon
    {
        string Id { get; set; }
        GameEnum Game { get; set; }
        AddonTypeEnum AddonType { get; set; }
        string Title { get; set; }
        string? Author { get; set; }
        string Description { get; set; }
        string DownloadUrl { get; set; }
        long FileSize { get; set; }
        string FileSizeString { get; }
        string Version { get; set; }
        bool HasNewerVersion { get; set; }
        bool IsInstalled { get; set; }
        string Status { get; }

        string ToMarkdownString();
    }
}