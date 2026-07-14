using System.Diagnostics;

namespace Core.Client.Interfaces;

/// <summary>Abstraction over <see cref="Process.Start"/> for testability.</summary>
public interface IProcessRunner
{
    /// <summary>Starts a process with the given file name, arguments, and working directory.</summary>
    /// <param name="fileName">Executable file name.</param>
    /// <param name="arguments">Command-line arguments.</param>
    /// <param name="workingDirectory">Working directory for the process.</param>
    /// <returns>A disposable result handle, or <see langword="null"/> if the process failed to start.</returns>
    IProcessRunResult? Start(string fileName, string arguments, string workingDirectory);

    /// <summary>Asynchronously waits for the process to exit.</summary>
    /// <param name="process">Process result handle returned by <see cref="Start"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    Task WaitForExitAsync(IProcessRunResult process, CancellationToken ct = default);
}

/// <summary>Represents a started process and provides access to its exit information.</summary>
public interface IProcessRunResult : IDisposable
{
    /// <summary>Gets the process ID.</summary>
    int Id { get; }

    /// <summary>Gets whether the process has exited.</summary>
    bool HasExited { get; }

    /// <summary>Gets the exit code of the process.</summary>
    int ExitCode { get; }
}
