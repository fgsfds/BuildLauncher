using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Common.All;
using Common.All.Enums;
using Common.All.Interfaces;

namespace Avalonia.Desktop.Helpers;

public sealed class SeparatorItem : IAddon
{
    [SetsRequiredMembers]
    public SeparatorItem()
    {
    }

    public AddonId AddonId { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public AddonTypeEnum Type { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public GameStruct SupportedGame { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public ImmutableArray<FeatureEnum>? RequiredFeatures { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string Title { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string? Author { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string? Description { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string? PathToFile { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string? FileName => throw new NotImplementedException();
    public long? GridImageHash { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public long? PreviewImageHash { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public IReadOnlyDictionary<string, string?>? DependentAddons { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public IReadOnlyDictionary<string, string?>? IncompatibleAddons { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public string? MainDef { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public ImmutableArray<string>? AdditionalDefs { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public IStartMap? StartMap { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public IReadOnlyDictionary<OSEnum, string>? Executables { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public bool IsFolder { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public bool IsFavorite { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ToMarkdownString() => throw new NotImplementedException();
}
