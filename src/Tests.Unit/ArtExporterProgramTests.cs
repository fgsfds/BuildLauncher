using System.Diagnostics;
using Addon.ArtExporter;

namespace Tests.Unit;

/// <summary>Verifies the ART exporter's command-line behavior.</summary>
public sealed class ArtExporterProgramTests
{
    /// <summary>Reports an unrecognized manifest game as a validation error.</summary>
    /// <returns>A task that completes when the CLI exits and its diagnostic is checked.</returns>
    [Fact]
    public async Task ValidateAddon_UnrecognizedGame_ReturnsValidationErrorAsync()
    {
        var package = Directory.CreateTempSubdirectory("BuildLauncher-ArtExporterProgramTests-");
        try
        {
            File.WriteAllText(Path.Combine(package.FullName, "addon.json"), """
                {
                    "id": "example",
                    "type": "tc",
                    "game": { "name": "MadeUpGame" },
                    "title": "Example",
                    "version": "1.0"
                }
                """);

            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet",
                UseShellExecute = false,
                RedirectStandardError = true
            };
            startInfo.ArgumentList.Add(typeof(ArtResourceExporter).Assembly.Location);
            startInfo.ArgumentList.Add("validate-addon");
            startInfo.ArgumentList.Add("--input");
            startInfo.ArgumentList.Add(package.FullName);

            using var process = Process.Start(startInfo);
            Assert.NotNull(process);
            var cancellationToken = TestContext.Current.CancellationToken;
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            Assert.Equal(1, process.ExitCode);
            Assert.Contains("'MadeUpGame' is not a recognized game", error, StringComparison.Ordinal);
        }
        finally
        {
            package.Delete(recursive: true);
        }
    }
}
