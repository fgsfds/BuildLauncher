using Core.Client.Interfaces;

namespace Tests.Unit.Helpers;

public sealed class FakeProcessRunner : IProcessRunner
{
    public string? LastFileName { get; private set; }
    public string? LastArguments { get; private set; }
    public string? LastWorkingDirectory { get; private set; }
    public int ExitCodeToReturn { get; set; }
    public bool ShouldReturnNull { get; set; }

    public IProcessRunResult? Start(string fileName, string arguments, string workingDirectory)
    {
        if (ShouldReturnNull)
        {
            return null;
        }

        LastFileName = fileName;
        LastArguments = arguments;
        LastWorkingDirectory = workingDirectory;

        return new FakeProcessRunResult(this, ExitCodeToReturn);
    }

    public Task WaitForExitAsync(IProcessRunResult process, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }


    private sealed class FakeProcessRunResult : IProcessRunResult
    {
        private readonly FakeProcessRunner _runner;

        public FakeProcessRunResult(FakeProcessRunner runner, int exitCode)
        {
            _runner = runner;
            ExitCode = exitCode;
        }

        public int Id => 42;
        public bool HasExited => true;
        public int ExitCode { get; }

        public void Dispose() { }
    }
}
