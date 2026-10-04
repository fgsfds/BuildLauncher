using System.Text.Json;
using Core.All.Helpers;
using Core.All.Serializable.Addon;

namespace Addon.ArtExporter;

/// <summary>Command-line entry point for standalone resource conversion.</summary>
internal static class Program
{
    /// <summary>Parses options, exports resources and returns a usable process status.</summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>Zero for success/help, two for usage errors, or one for export failures.</returns>
    private static int Main(string[] args)
    {
        if (args.Length == 0 || args.Contains("--help", StringComparer.Ordinal))
        {
            Console.WriteLine("""
                BuildLauncher ART exporter (.NET 10)
                Usage: --input DIRECTORY --output NEW_DIRECTORY --palette GAME.act
                       [--surface SURFACE.DAT] [--def resources.def] [--ifcrc]
                Converts root ART files and optional SURFACE.DAT into tiles/*.png and DEF.
                Output must not exist; its parent must exist. The source is never changed.
                VOXEL.DAT conversion is reserved for a later separate DEF exporter.
                Manifest validation: validate-addon --input PACKAGE_DIRECTORY [--manifest addon.json]
                """);
            return args.Length == 0 ? 2 : 0;
        }

        try
        {
            if (args[0] == "validate-addon")
            {
                return ValidateAddon(args[1..]);
            }

            var options = Parse(args);
            var report = ArtResourceExporter.Export(options);
            Console.WriteLine($"Exported {report.TileCount} tiles from {report.ArtFileCount} ART files, {report.AnimationCount} animations and {report.EmptyTileCount} empty slots skipped.");
            Console.WriteLine($"Output: {Path.GetFullPath(options.OutputDirectory)}");
            return 0;
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or JsonException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }

    /// <summary>Parses a manifest and applies the shared developer-editor authoring rules.</summary>
    /// <param name="args">The validation command options.</param>
    /// <returns>Zero for valid input, one for a validation failure.</returns>
    private static int ValidateAddon(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < args.Length; index++)
        {
            var name = args[index];
            if (name is not ("--input" or "--manifest") || ++index >= args.Length ||
                args[index].StartsWith("--", StringComparison.Ordinal) || !values.TryAdd(name, args[index]))
            {
                throw new ArgumentException("Use validate-addon --input DIRECTORY [--manifest addon.json] without duplicate flags.", nameof(args));
            }
        }

        if (!values.TryGetValue("--input", out var input))
        {
            throw new ArgumentException("Required option missing: --input", nameof(args));
        }

        var fileName = values.GetValueOrDefault("--manifest") ?? "addon.json";
        if (fileName.IndexOfAny(['/', '\\', ':']) >= 0 || !fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The manifest must be a root filename ending in .json.", nameof(args));
        }

        using var stream = File.OpenRead(Path.Combine(input, fileName));
        var manifest = JsonSerializer.Deserialize(stream, AddonManifestJsonContext.Default.AddonManifestJsonModel);
        if (manifest is null)
        {
            throw new InvalidDataException("The manifest must contain an addon object, not null.");
        }

        var result = AddonManifestValidator.Validate(manifest, Directory.EnumerateFiles(input).Select(static path => Path.GetFileName(path)!));
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine(result.Message);
            return 1;
        }

        Console.WriteLine(result.Message);
        return 0;
    }

    /// <summary>Accepts only supported flags, rejecting duplicates and missing values.</summary>
    /// <param name="args">The supplied arguments.</param>
    /// <returns>The validated option names and values.</returns>
    private static ExportOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var includeCrc = false;
        for (var index = 0; index < args.Length; index++)
        {
            var name = args[index];
            if (name == "--ifcrc")
            {
                if (includeCrc)
                {
                    throw new ArgumentException("Duplicate --ifcrc flag.", nameof(args));
                }

                includeCrc = true;
                continue;
            }

            if (name is not ("--input" or "--output" or "--palette" or "--surface" or "--def"))
            {
                throw new ArgumentException($"Unknown option: {name}", nameof(args));
            }

            if (++index >= args.Length || args[index].StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Missing value for {name}.", nameof(args));
            }

            if (!values.TryAdd(name, args[index]))
            {
                throw new ArgumentException($"Duplicate option: {name}", nameof(args));
            }
        }

        foreach (var required in new[] { "--input", "--output", "--palette" })
        {
            if (!values.ContainsKey(required))
            {
                throw new ArgumentException($"Required option missing: {required}", nameof(args));
            }
        }

        return new ExportOptions
        {
            InputDirectory = values["--input"],
            OutputDirectory = values["--output"],
            PalettePath = values["--palette"],
            SurfacePath = values.GetValueOrDefault("--surface"),
            DefFileName = values.GetValueOrDefault("--def") ?? "resources.def",
            IncludeCrc = includeCrc
        };
    }
}
