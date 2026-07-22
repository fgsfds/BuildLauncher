using Core.All.Enums;
using Games.Games;

namespace Tests.Unit;

/// <summary>
///     Tests for <see cref="BaseGame.GenerateNumberedFiles" />.
/// </summary>
public sealed class GenerateNumberedFilesTests
{
    /// <summary>
    ///     Tests default zero-padded format (padWidth=3).
    /// </summary>
    [Fact]
    public void DefaultPadWidth_ProducesThreeDigitZeroPadded()
    {
        var result = TestGame.Generate("FILE", "EXT", 0, 3);

        Assert.Equal(
            [
                "FILE000.EXT",
                "FILE001.EXT",
                "FILE002.EXT"
            ], result
            );
    }

    /// <summary>
    ///     Tests that padWidth 0 produces no padding.
    /// </summary>
    [Fact]
    public void PadWidthZero_ProducesNoPadding()
    {
        var result = TestGame.Generate("LEVEL", "MAP", 1, 5, 0);

        Assert.Equal(
            [
                "LEVEL1.MAP",
                "LEVEL2.MAP",
                "LEVEL3.MAP",
                "LEVEL4.MAP"
            ], result
            );
    }

    /// <summary>
    ///     Tests that padWidth 2 produces two-digit zero-padded output.
    /// </summary>
    [Fact]
    public void PadWidthTwo_ProducesTwoDigitPadded()
    {
        var result = TestGame.Generate("TEX", "ART", 0, 3, 2);

        Assert.Equal(
            [
                "TEX00.ART",
                "TEX01.ART",
                "TEX02.ART"
            ], result
            );
    }

    /// <summary>
    ///     Tests that a single-element range returns one file.
    /// </summary>
    [Fact]
    public void SingleElementRange_ReturnsOneFile()
    {
        var result = TestGame.Generate("X", "Y", 5, 6, 0);
        Assert.Single(result);
        Assert.Equal("X5.Y", result[0]);
    }

    /// <summary>
    ///     Tests that an empty range returns an empty list.
    /// </summary>
    [Fact]
    public void EmptyRange_ReturnsEmptyList()
    {
        var result = TestGame.Generate("X", "Y", 0, 0, 3);
        Assert.Empty(result);
    }

    /// <summary>
    ///     Tests that the TILES format used by Witchaven matches expected naming.
    /// </summary>
    [Fact]
    public void WitchavenTilesFormat_MatchesExpectedPattern()
    {
        var result = TestGame.Generate("TILES", "ART", 0, 11);
        Assert.Equal(11, result.Count);
        Assert.Equal("TILES000.ART", result[0]);
        Assert.Equal("TILES010.ART", result[^1]);
    }

    /// <summary>
    ///     Tests that the LEVEL format used by Witchaven matches expected naming.
    /// </summary>
    [Fact]
    public void WitchavenLevelFormat_MatchesExpectedPattern()
    {
        var result = TestGame.Generate("LEVEL", "MAP", 1, 26, 0);
        Assert.Equal(25, result.Count);
        Assert.Equal("LEVEL1.MAP", result[0]);
        Assert.Equal("LEVEL25.MAP", result[^1]);
    }


    /// <summary>
    ///     Concrete test subclass that exposes <see cref="BaseGame.GenerateNumberedFiles" />.
    /// </summary>
    private sealed class TestGame : BaseGame
    {
        /// <inheritdoc />
        public override GameEnum GameEnum => GameEnum.Standalone;

        /// <inheritdoc />
        public override string FullName => "Test";

        /// <inheritdoc />
        public override string ShortName => "Test";

        /// <inheritdoc />
        protected override IReadOnlyList<string> RequiredFiles => [];

        /// <inheritdoc />
        public override Enum? Skills => null;

        /// <summary>
        ///     Exposes <see cref="BaseGame.GenerateNumberedFiles" /> for testing.
        /// </summary>
        public static IReadOnlyList<string> Generate(string baseName, string extension, int start, int endExclusive, int padWidth = 3)
        {
            return GenerateNumberedFiles(baseName, extension, start, endExclusive, padWidth);
        }
    }
}
