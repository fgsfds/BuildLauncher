using Addons.Addons;
using Core.All.Enums;

namespace Tests.Unit;

/// <summary>
///     Tests for the <see cref="BaseAddon" /> class.
/// </summary>
public sealed class BaseAddonTests
{
    /// <summary>
    ///     Creates a test <see cref="DukeCampaign" /> with the specified parameters.
    /// </summary>
    private static DukeCampaign CreateAddon(
        string? id = null,
        string? version = null,
        string? title = null,
        string? author = null,
        DateOnly? releaseDate = null,
        string? description = null,
        AddonTypeEnum type = AddonTypeEnum.Mod,
        IReadOnlyDictionary<string, string?>? dependentAddons = null,
        IReadOnlyDictionary<string, string?>? incompatibleAddons = null)
    {
        return new DukeCampaign
        {
            AddonId = new(id ?? "test-addon", version),
            FileInfo = null,
            Type = type,
            SupportedGame = new(GameEnum.Duke3D),
            Title = title ?? "Test Addon",
            Author = author,
            ReleaseDate = releaseDate,
            Description = description,
            RequiredFeatures = null,
            DependentAddons = dependentAddons,
            IncompatibleAddons = incompatibleAddons,
            GridImageHash = null,
            PreviewImageHash = null,
            MainDef = null,
            AdditionalDefs = null,
            StartMap = null,
            Executables = null,
            Options = null,
            MainCon = null,
            AdditionalCons = null,
            RTS = null
        };
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> returns only the title for a minimal addon.
    /// </summary>
    [Fact]
    public void ToMarkdownString_Minimal_ReturnsOnlyTitle()
    {
        var addon = CreateAddon(title: "Minimal");

        var result = addon.ToMarkdownString();

        Assert.Equal("## Minimal", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> includes the version when set.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithVersion_IncludesVersion()
    {
        var addon = CreateAddon(version: "2.1");

        var result = addon.ToMarkdownString();

        Assert.Equal($"## Test Addon{Environment.NewLine}{Environment.NewLine}#### v2.1", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> includes author and date when set.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithAuthorAndDate_IncludesBoth()
    {
        var addon = CreateAddon(title: "My Addon", version: "1.0", author: "Tester", releaseDate: new(2024, 3, 15));

        var result = addon.ToMarkdownString();

        var nl = Environment.NewLine;
        Assert.Equal($"## My Addon{nl}{nl}#### v1.0{nl}{nl}*Released on:* 15.03.2024{nl}{nl}*by Tester*", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> joins description lines without URLs.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithDescriptionWithoutUrls_JoinsLines()
    {
        var addon = CreateAddon(title: "Desc", version: "1.0", description: "Line one\nLine two\nLine three");

        var result = addon.ToMarkdownString();

        var nl = Environment.NewLine;
        Assert.Equal($"## Desc{nl}{nl}#### v1.0{nl}{nl}Line one{nl}{nl}Line two{nl}{nl}Line three", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> converts URL lines to markdown links.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithUrlLineInDescription_ConvertsToMarkdownLink()
    {
        var addon = CreateAddon(title: "Url", version: "1.0", description: "https://example.com");

        var result = addon.ToMarkdownString();

        var nl = Environment.NewLine;
        Assert.Equal($"## Url{nl}{nl}#### v1.0{nl}{nl}[https://example.com](https://example.com)", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> converts only URL lines in a mixed description.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithUrlAmongLines_ConvertsOnlyUrlLines()
    {
        var addon = CreateAddon(title: "Mix", version: "1.0", description: "Normal text\nhttps://example.com\nMore text");

        var result = addon.ToMarkdownString();

        var nl = Environment.NewLine;
        Assert.Equal($"## Mix{nl}{nl}#### v1.0{nl}{nl}Normal text{nl}{nl}[https://example.com](https://example.com){nl}{nl}More text", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> includes the Requires section for addons with dependencies.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithDependencies_IncludesRequiresSection()
    {
        var addon = CreateAddon(
            type: AddonTypeEnum.TC,
            dependentAddons: new Dictionary<string, string?>
            {
                {
                    "dep-one", null
                },
                {
                    "dep-two", "1.5"
                }
            }
            );

        var result = addon.ToMarkdownString();

        Assert.Contains("#### Requires:", result);
        Assert.Contains("dep-one", result);
        Assert.Contains("dep-two", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> omits the Requires section for official addons.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithDependencies_OfficialType_OmitsRequiresSection()
    {
        var addon = CreateAddon(
            type: AddonTypeEnum.Official,
            dependentAddons: new Dictionary<string, string?>
            {
                {
                    "some-dep", null
                }
            }
            );

        var result = addon.ToMarkdownString();

        Assert.DoesNotContain("#### Requires:", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> includes the Incompatible section for addons with incompatibles.
    /// </summary>
    [Fact]
    public void ToMarkdownString_WithIncompatibleAddons_IncludesIncompatibleSection()
    {
        var addon = CreateAddon(
            incompatibleAddons: new Dictionary<string, string?>
            {
                {
                    "bad-mod", null
                },
                {
                    "old-mod", "<=1.0"
                }
            }
            );

        var result = addon.ToMarkdownString();

        Assert.Contains("#### Incompatible with:", result);
        Assert.Contains("bad-mod", result);
        Assert.Contains("old-mod", result);
    }

    /// <summary>
    ///     Tests that <see cref="BaseAddon.ToMarkdownString" /> returns complete markdown when all fields are set.
    /// </summary>
    [Fact]
    public void ToMarkdownString_AllFields_ReturnsCompleteMarkdown()
    {
        var addon = CreateAddon(
            id: "full-addon", version: "3.0", title: "Full Addon",
            author: "Creator", releaseDate: new(2023, 12, 1),
            description: "First line\nhttps://example.com\nLast line",
            type: AddonTypeEnum.TC,
            dependentAddons: new Dictionary<string, string?>
            {
                {
                    "required-dep", null
                }
            },
            incompatibleAddons: new Dictionary<string, string?>
            {
                {
                    "conflict-mod", null
                }
            }
            );

        var result = addon.ToMarkdownString();

        var nl = Environment.NewLine;
        Assert.StartsWith("## Full Addon", result);
        Assert.Contains($"{nl}{nl}#### v3.0", result);
        Assert.Contains($"{nl}{nl}*Released on:* 01.12.2023", result);
        Assert.Contains($"{nl}{nl}*by Creator*", result);
        Assert.Contains($"{nl}{nl}First line{nl}{nl}[https://example.com](https://example.com){nl}{nl}Last line", result);
        Assert.Contains($"{nl}{nl}#### Requires:{nl}required-dep", result);
        Assert.Contains($"{nl}{nl}#### Incompatible with:{nl}conflict-mod", result);
    }
}
