using System.Diagnostics;
using Core.Client.Interfaces;

namespace Core.Client.Helpers;

/// <summary>Default <see cref="IProcessRunner"/> implementation that uses <see cref="Process.Start"/>.</summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <summary>Singleton instance of <see cref="ProcessRunner"/>.</summary>
    public static readonly IProcessRunner Instance = new ProcessRunner();

    /// <inheritdoc />
    public IProcessRunResult? Start(string fileName, string arguments, string workingDirectory)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = true
        };

        var process = Process.Start(psi);

        if (process is null)
        {
            return null;
        }

        return new ProcessRunResult(process);
    }

    /// <inheritdoc />
    public async Task WaitForExitAsync(IProcessRunResult process, CancellationToken ct = default)
    {
        var real = ((ProcessRunResult)process)._process;
        await real.WaitForExitAsync(ct).ConfigureAwait(false);
    }


    private sealed class ProcessRunResult : IProcessRunResult
    {
        internal readonly Process _process;

        public ProcessRunResult(Process process) => _process = process;

        public int Id => _process.Id;
        public bool HasExited => _process.HasExited;
        public int ExitCode => _process.ExitCode;

        public void Dispose() => _process.Dispose();
    }
}
