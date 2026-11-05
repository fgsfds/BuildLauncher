#requires -Version 7.0
<#
.SYNOPSIS
Runs two native UI smoke-test launches of an existing published build in a fresh directory.
#>
param(
    [Parameter(Mandatory)][string]$PublishDirectory,
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [int]$TimeoutSeconds = 120
)
$ErrorActionPreference = 'Stop'
$publish = (Resolve-Path -LiteralPath $PublishDirectory).Path
$results = [IO.Path]::GetFullPath($ResultsDirectory)
if (Test-Path -LiteralPath $results) { throw "Results directory must not exist: $results" }
$null = New-Item -ItemType Directory -Path $results
Copy-Item -Path (Join-Path $publish '*') -Destination $results -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot '../db') -Destination $results -Recurse
New-Item -ItemType File -Path (Join-Path $results '.published-smoke-test') | Out-Null
$executable = Join-Path $results $(if ($IsWindows) { 'BuildLauncher.exe' } else { 'BuildLauncher' })
if (!(Test-Path -LiteralPath $executable)) { throw "Missing published executable: $executable" }
foreach ($phase in @('first', 'restart')) {
    $argument = if ($phase -eq 'first') { '--smoke-test' } else { '--smoke-test-restart' }
    $start = @{
        FilePath = $executable
        ArgumentList = @('--offline', $argument)
        WorkingDirectory = $results
        PassThru = $true
        RedirectStandardOutput = (Join-Path $results "$phase.stdout.log")
        RedirectStandardError = (Join-Path $results "$phase.stderr.log")
    }
    if ($IsWindows) { $start.WindowStyle = 'Hidden' }
    $process = Start-Process @start
    try {
        if (!$process.WaitForExit($TimeoutSeconds * 1000)) {
            $process.Kill($true)
            $process.WaitForExit()
            throw "$phase launch timed out after $TimeoutSeconds seconds."
        }
        if ($process.ExitCode -ne 0) { throw "$phase launch exited with code $($process.ExitCode)." }
        if (Test-Path -LiteralPath (Join-Path $results 'smoke-error.txt')) {
            throw (Get-Content -LiteralPath (Join-Path $results 'smoke-error.txt') -Raw)
        }
        if (!(Test-Path -LiteralPath (Join-Path $results "$phase.passed"))) { throw "$phase launch did not finish its checks." }
        if (Get-ChildItem -LiteralPath $results -Filter '*.crashlog') { throw "$phase launch produced a crash log." }
        Copy-Item -LiteralPath (Join-Path $results 'BuildLauncher.log') -Destination (Join-Path $results "$phase.application.log")
        Write-Host "$phase published-build checks passed."
    }
    finally { $process.Dispose() }
}
