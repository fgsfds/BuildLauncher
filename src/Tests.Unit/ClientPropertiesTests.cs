using Core.Client.Helpers;

namespace Tests.Unit;

/// <summary> Verifies published and development database file discovery across filesystem layouts. </summary>
public sealed class ClientPropertiesTests
{
    /// <summary> Adjacent data wins over repository data regardless of output depth. </summary>
    /// <param name="fileName">The database file to resolve.</param>
    [Theory]
    [InlineData("addons.json")]
    [InlineData("data.json")]
    [InlineData("manifests.json")]
    public void FindLocalDatabaseFile_PrefersPublishedDataThenRepository(string fileName)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var output = Path.Combine(root, "custom output", "deep", "linux-x64");
        _ = Directory.CreateDirectory(Path.Combine(output, "db"));
        _ = Directory.CreateDirectory(Path.Combine(root, "db"));
        try
        {
            File.WriteAllText(Path.Combine(root, "BuildLauncher.slnx"), "");
            var repositoryFile = Path.Combine(root, "db", fileName);
            var publishedFile = Path.Combine(output, "db", fileName);
            File.WriteAllText(repositoryFile, "repository");
            File.WriteAllText(publishedFile, "published");
            Assert.Equal(publishedFile, ClientProperties.FindLocalDatabaseFile(output, fileName));
            File.Delete(publishedFile);
            Assert.Equal(repositoryFile, ClientProperties.FindLocalDatabaseFile(output, fileName));
            File.Delete(Path.Combine(root, "BuildLauncher.slnx"));
            Assert.Null(ClientProperties.FindLocalDatabaseFile(output, fileName));
        }
        finally { Directory.Delete(root, true); }
    }
}
