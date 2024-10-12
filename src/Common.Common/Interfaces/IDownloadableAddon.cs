using Common.Enums;

namespace Common.Interfaces;

public interface IDownloadableAddon
{
    string Id { get; set; }
    GameEnum Game { get; set; }
    AddonTypeEnum AddonType { get; set; }
    string Title { get; set; }
    Uri DownloadUrl { get; set; }
    long FileSize { get; set; }
    string Version { get; set; }
    bool HasNewerVersion { get; set; }
    bool IsInstalled { get; set; }
    string Status { get; }
    string? Author { get; set; }
    string? Description { get; set; }
    int Installs { get; set; }
    decimal Rating { get; set; }
    DateTime UpdateDate { get; set; }

    string FileSizeString { get; }
    string UpdateDateString { get; }
    string RatingStr { get; }

    string ToMarkdownString();
}