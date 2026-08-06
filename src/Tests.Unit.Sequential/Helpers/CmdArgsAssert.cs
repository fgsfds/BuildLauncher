namespace Tests.Unit.Helpers;

/// <summary>
///     Assertions for command-line arguments tests.
/// </summary>
internal static class CmdArgsAssert
{
    /// <summary>
    ///     Normalizes path separators in both the actual and expected arguments
    ///     and asserts they are equal in the exact order produced.
    /// </summary>
    /// <param name="expected">
    ///     Expected command-line arguments.
    /// </param>
    /// <param name="args">
    ///     Actual command-line arguments.
    /// </param>
    public static void Equal(string expected, string args)
    {
        NormalizerHelper.NormalizeExpectedArgs(ref args, ref expected);

        Assert.Equal(expected, args);
    }
}
