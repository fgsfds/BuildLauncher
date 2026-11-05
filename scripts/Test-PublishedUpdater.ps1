#requires -Version 7.0
<#
.SYNOPSIS
Verifies real published executable updates using a loopback HTTP server and isolated installations.
#>
param(
    [Parameter(Mandatory)][string]$PublishDirectory,
    [Parameter(Mandatory)][string]$BaselineDirectory,
    [Parameter(Mandatory)][string]$VerifiedPublishResults,
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [int]$TimeoutSeconds = 120
)
$ErrorActionPreference = 'Stop'
$results = [IO.Path]::GetFullPath($ResultsDirectory)
if (Test-Path -LiteralPath $results) { throw "Results directory must not exist: $results" }
$null = New-Item -ItemType Directory -Path $results
$publish = (Resolve-Path -LiteralPath $PublishDirectory).Path
$expectedVersion = (Get-Content (Join-Path $VerifiedPublishResults 'first.passed') -Raw).Trim()
$exeName = if ($IsWindows) { 'BuildLauncher.exe' } else { 'BuildLauncher' }
$targetHash = (Get-FileHash (Join-Path $publish $exeName)).Hash
$seed = Join-Path $results 'seed'
& "$PSScriptRoot/Test-PublishedBuild.ps1" -PublishDirectory $BaselineDirectory -ResultsDirectory $seed -TimeoutSeconds $TimeoutSeconds
$oldVersion = (Get-Content (Join-Path $seed 'first.passed') -Raw).Trim()
if ([version]$expectedVersion -le [version]$oldVersion) { throw 'The target must be newer than the baseline.' }
$oldHash = (Get-FileHash (Join-Path $seed $exeName)).Hash
if ($oldHash -eq $targetHash) { throw 'Update fixtures must contain different executables.' }

$payload = Join-Path $results 'payload'
$null = New-Item -ItemType Directory -Path $payload
Copy-Item -LiteralPath (Join-Path $publish $exeName) -Destination $payload
Set-Content (Join-Path $payload 'companion.txt') 'Updated companion file'
$archive = Join-Path $results 'update.zip'
[IO.Compression.ZipFile]::CreateFromDirectory($payload, $archive)
Set-Content (Join-Path $payload 'zz-blocked') 'Collision fixture'
[IO.Compression.ZipFile]::CreateFromDirectory($payload, (Join-Path $results 'collision.zip'))
# Release ZIPs may not carry Unix execute permissions. The updater must restore them.
foreach ($zipPath in @($archive, (Join-Path $results 'collision.zip'))) {
    $zip = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Update)
    try { $zip.GetEntry($exeName).ExternalAttributes = 0 }
    finally { $zip.Dispose() }
}
$hash = (Get-FileHash $archive).Hash
Set-Content (Join-Path $results 'corrupt.zip') 'This is not a ZIP archive'

$server = Start-ThreadJob -ArgumentList $results -ScriptBlock {
    param($root)
    $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    try {
        [IO.File]::WriteAllText((Join-Path $root 'port.txt'), $listener.LocalEndpoint.Port.ToString())
        while ($true) {
            if (!$listener.Pending()) { Start-Sleep -Milliseconds 20; continue }
            $client = $listener.AcceptTcpClient()
            try {
                $client.ReceiveTimeout = 10000
                $stream = $client.GetStream()
                $reader = [IO.StreamReader]::new($stream, [Text.Encoding]::ASCII, $false, 1024, $true)
                try {
                    $request = $reader.ReadLine()
                    $range = $false
                    while ($line = $reader.ReadLine()) { if ($line.StartsWith('Range:', [StringComparison]::OrdinalIgnoreCase)) { $range = $true } }
                    $route = $request.Split(' ')[1]
                    [IO.File]::AppendAllText((Join-Path $root 'server.log'), "$route range=$range`n")
                    if ($route -eq '/interrupt.zip' -and $range) {
                        $header = [Text.Encoding]::ASCII.GetBytes("HTTP/1.1 503 Service Unavailable`r`nContent-Length: 0`r`nConnection: close`r`n`r`n")
                        $stream.Write($header, 0, $header.Length)
                    }
                    else {
                        $file = switch ($route) {
                            '/collision.zip' { 'collision.zip' }
                            '/corrupt.zip' { 'corrupt.zip' }
                            default { 'update.zip' }
                        }
                        $inputFile = [IO.File]::OpenRead((Join-Path $root $file))
                        try {
                            $header = [Text.Encoding]::ASCII.GetBytes("HTTP/1.1 200 OK`r`nContent-Length: $($inputFile.Length)`r`nConnection: close`r`n`r`n")
                            $stream.Write($header, 0, $header.Length)
                            if ($route -eq '/interrupt.zip') {
                                $buffer = [byte[]]::new(4096)
                                $count = $inputFile.Read($buffer, 0, $buffer.Length)
                                $stream.Write($buffer, 0, $count)
                            }
                            else { $inputFile.CopyTo($stream) }
                        }
                        finally { $inputFile.Dispose() }
                    }
                }
                finally { $reader.Dispose() }
            }
            catch { [IO.File]::AppendAllText((Join-Path $root 'server.log'), "Server: $_`n") }
            finally { $client.Dispose() }
        }
    }
    finally { $listener.Stop() }
}

function Invoke-TestProcess([string]$Folder, [string]$Argument, [string]$LogName) {
    $start = @{
        FilePath = (Join-Path $Folder $exeName)
        ArgumentList = @('--offline', $Argument)
        WorkingDirectory = $Folder
        PassThru = $true
        RedirectStandardOutput = (Join-Path $Folder "$LogName.stdout.log")
        RedirectStandardError = (Join-Path $Folder "$LogName.stderr.log")
    }
    if ($IsWindows) { $start.WindowStyle = 'Hidden' }
    $process = Start-Process @start
    try {
        if (!$process.WaitForExit($TimeoutSeconds * 1000)) {
            $process.Kill($true)
            $process.WaitForExit()
            throw "$LogName timed out."
        }
        return $process.ExitCode
    }
    finally { $process.Dispose() }
}

try {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    while (!(Test-Path (Join-Path $results 'port.txt'))) {
        if ([DateTime]::UtcNow -gt $deadline -or $server.State -eq 'Failed') { throw 'Loopback server did not start.' }
        Start-Sleep -Milliseconds 100
    }
    $port = Get-Content (Join-Path $results 'port.txt')
    foreach ($scenario in @('hash', 'corrupt', 'interrupt', 'collision', 'success')) {
        $folder = Join-Path $results $scenario
        Copy-Item -LiteralPath $seed -Destination $folder -Recurse
        Remove-Item -LiteralPath (Join-Path $folder 'restart.passed')
        Set-Content (Join-Path $folder 'user-file.txt') 'Keep this user content'
        $databaseHash = (Get-FileHash (Join-Path $folder 'BuildLauncher.db')).Hash
        $route = switch ($scenario) {
            'corrupt' { 'corrupt.zip' }
            'interrupt' { 'interrupt.zip' }
            'collision' { 'collision.zip' }
            default { 'update.zip' }
        }
        $releaseHash = switch ($scenario) {
            'hash' { '00' * 32 }
            'corrupt' { (Get-FileHash (Join-Path $results 'corrupt.zip')).Hash }
            'collision' { (Get-FileHash (Join-Path $results 'collision.zip')).Hash }
            default { $hash }
        }
        if ($scenario -eq 'collision') { $null = New-Item -ItemType Directory -Path (Join-Path $folder 'zz-blocked') }
        $metadata = @(@{
            SupportedOS = $(if ($IsWindows) { 'Windows' } else { 'Linux' })
            Version = $expectedVersion
            Description = 'Local integration-test fixture'
            DownloadUrl = "http://127.0.0.1:$port/$route"
            Hash = "sha256:$releaseHash"
        })
        ConvertTo-Json -InputObject $metadata | Set-Content (Join-Path $folder '.smoke-update.json')
        $exitCode = Invoke-TestProcess $folder '--smoke-update' 'update'
        if ($scenario -eq 'success') {
            if ($exitCode -ne 0) { throw "Successful update exited with $exitCode." }
            if ((Get-FileHash (Join-Path $folder $exeName)).Hash -ne $targetHash) { throw 'Installed executable hash mismatch.' }
            if (Test-Path (Join-Path $folder '.update')) { throw 'Successful update left its marker behind.' }
            if ((Get-Content (Join-Path $folder 'companion.txt') -Raw).Trim() -ne 'Updated companion file') { throw 'Companion file was not installed.' }
            if (!$IsWindows) {
                if (Test-Path (Join-Path $folder 'restart.passed')) { throw 'Linux unexpectedly relaunched the app.' }
                if ((Invoke-TestProcess $folder '--smoke-test-restart' 'manual-restart') -ne 0) { throw 'Manual Linux restart failed.' }
            }
            # Capture a process handle while the restarted app is alive; late Get-Process snapshots can lose ExitCode.
            $restarted = @()
            if ($IsWindows) {
                foreach ($candidate in @(Get-Process -Name BuildLauncher -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq (Join-Path $folder $exeName) })) {
                    try { $null = $candidate.Handle; $restarted += $candidate }
                    catch [InvalidOperationException] { $candidate.Dispose() }
                }
            }
            $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
            while (!(Test-Path (Join-Path $folder 'restart.passed'))) {
                if (Test-Path (Join-Path $folder 'smoke-error.txt')) { throw (Get-Content (Join-Path $folder 'smoke-error.txt') -Raw) }
                if ([DateTime]::UtcNow -gt $deadline) { throw 'Updated Windows app did not restart and finish its checks.' }
                Start-Sleep -Milliseconds 100
            }
            if ((Get-Content (Join-Path $folder 'restart.passed') -Raw).Trim() -ne $expectedVersion) { throw 'Restarted app has the wrong version.' }
            if ($IsWindows) {
                foreach ($child in $restarted) {
                    try {
                        if (!$child.WaitForExit($TimeoutSeconds * 1000)) { throw 'Updated app did not exit after its checks.' }
                        if ($child.ExitCode -ne 0) { throw "Updated app failed during shutdown: exit code $($child.ExitCode)." }
                    }
                    finally { $child.Dispose() }
                }
            }
        }
        else {
            if ($exitCode -eq 0) { throw "$scenario unexpectedly succeeded." }
            $errorFiles = if ($scenario -eq 'collision') {
                @(Get-ChildItem -LiteralPath $folder -Filter '*.update_crashlog')
            } else { @(Get-Item -LiteralPath (Join-Path $folder 'smoke-error.txt')) }
            $errorText = ($errorFiles | Get-Content -Raw) -join "
"
            $expectedError = switch ($scenario) {
                'hash' { 'SHA-256 digest does not match' }
                'corrupt' { 'InvalidDataException' }
                'interrupt' { 'HttpRequestException' }
                'collision' { 'IOException' }
            }
            if ($errorText -notmatch $expectedError) { throw "$scenario failed for an unexpected reason: $errorText" }
            if ((Get-FileHash (Join-Path $folder $exeName)).Hash -ne $oldHash) { throw "$scenario damaged the old executable." }
            if ((Get-FileHash (Join-Path $folder 'BuildLauncher.db')).Hash -ne $databaseHash) { throw "$scenario changed the database." }
            if (Test-Path (Join-Path $folder 'companion.txt')) { throw "$scenario left partially installed files." }
            if ($scenario -eq 'collision') {
                if (!(Test-Path (Join-Path $folder '.update'))) { throw 'Failed installation lost its retry marker.' }
                # Keep the failed payload as evidence, but disable retry to verify the restored executable boots.
                Move-Item -LiteralPath (Join-Path $folder '.update') -Destination (Join-Path $folder 'failed-update-marker.txt')
            }
            elseif (Test-Path (Join-Path $folder '.update')) { throw "$scenario published an invalid update marker." }
            if (Test-Path (Join-Path $folder 'smoke-error.txt')) {
                Move-Item -LiteralPath (Join-Path $folder 'smoke-error.txt') -Destination (Join-Path $folder 'expected-update-error.txt')
            }
            if ((Invoke-TestProcess $folder '--smoke-test-restart' 'recovery') -ne 0) { throw "$scenario left an unbootable installation." }
            if ((Get-Content (Join-Path $folder 'restart.passed') -Raw).Trim() -ne $oldVersion) { throw "$scenario did not restore the old version." }
        }
        if ((Get-Content (Join-Path $folder 'user-file.txt') -Raw).Trim() -ne 'Keep this user content') { throw "$scenario changed user content." }
        if (Test-Path (Join-Path $folder 'smoke-error.txt')) { throw (Get-Content (Join-Path $folder 'smoke-error.txt') -Raw) }
        if ($scenario -eq 'interrupt' -and (Get-Content (Join-Path $results 'server.log') -Raw) -notmatch '/interrupt.zip range=True') {
            throw 'Interrupted transfer did not exercise the resume request.'
        }
        Set-Content (Join-Path $folder 'scenario.passed') $scenario
        Write-Host "$scenario published-updater checks passed."
    }
}
finally {
    Stop-Job $server
    Remove-Job $server -Force
    # The Windows restart is a child of the exited updater, so clean up only this test's exact executable path.
    if ($IsWindows) {
        $updatedExecutable = Join-Path (Join-Path $results 'success') $exeName
        Get-Process -Name BuildLauncher -ErrorAction SilentlyContinue |
            Where-Object { $_.Path -eq $updatedExecutable } |
            Stop-Process -Force
    }
}
