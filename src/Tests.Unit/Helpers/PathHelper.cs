namespace Tests.Unit.Helpers;

public static class PathHelper
{
    /// <summary>
    /// Returns a non-existent fake path for use in test addon files.
    /// </summary>
    public static string GetFakePath()
    {
        return Path.Combine("TestData", Guid.NewGuid().ToString());
    }
}
