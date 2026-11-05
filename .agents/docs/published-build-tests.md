# Published release checks

The reusable `published-build-tests.yml` workflow publishes the Windows and Linux
Release profiles and runs the actual self-contained executables. Linux uses Xvfb;
Windows uses the runner's native desktop. Trimming remains disabled.

Each test copies the published output to a fresh directory, adds the repository's
`db/` files as offline fixtures, and launches twice. The normal composition root,
SQLite migrations, native window and XAML resources run on both launches. The checks
verify consent, catalog deserialization, Settings/Ports/Tools/Standalone/About page
binding and layout, rendering to PNG, theme persistence, active-tab restoration and
saved window placement. Failures, missing completion markers, crash logs, nonzero
exit codes and a 120-second per-launch timeout fail the job. Logs and page images
are uploaded even on failure.

The `--smoke-test` and `--smoke-test-restart` switches are opt-in and require both
`--offline` and a `.published-smoke-test` marker next to the executable. Use the
script, which creates a fresh directory and never reuses an existing installation:

```powershell
dotnet publish src/Avalonia.Desktop/Avalonia.Desktop.csproj -p:PublishProfile=Windows -c Release -o artifacts/published-windows
./scripts/Test-PublishedBuild.ps1 -PublishDirectory artifacts/published-windows -ResultsDirectory artifacts/smoke-results
```

For Linux use the `Linux` profile and run the same script under
`xvfb-run --auto-servernum --server-args="-screen 0 1600x1000x24" pwsh -File`.
PowerShell 7, Xvfb and Avalonia's native Linux dependencies must be installed.

`Build and test` includes both published checks. `Create release` also calls the
reusable workflow, including for manual releases, using the triggering commit SHA.
Signing waits for the startup and updater checks on both operating systems to pass, then downloads the tested Windows
artifact instead of rebuilding it. The existing release remains Windows-only.

These are deterministic offline smoke tests, not exhaustive UI automation. They do
not test public release discovery, game launches, native file pickers, signing,
or all desktop/window-manager configurations. Existing unit and headless UI tests
remain necessary. The JSON fixtures are test inputs, not additions to release archives.

## Published updater tests

After startup checks, CI publishes the same revision with version 0.0.1 as the
older installation. The ordinary Release publish is the update target. These are
two real self-contained executables with different versions and hashes; this does
not claim compatibility with every historical BuildLauncher release.

The updater runner seeds the old version's real SQLite settings, then serves ZIP
payloads from an ephemeral loopback HTTP server. Only release metadata is supplied
as a fixture. The production HTTP downloader, SHA-256 check, extraction, staging,
running-executable replacement and rollback execute through the production implementation. The guarded
`--smoke-update` entry point requires the same isolated-directory marker and offline
flag as startup tests, and accepts only a loopback HTTP fixture URL.

Scenarios cover a successful update, bad digest, corrupt ZIP, truncated HTTP transfer
with a failed resume request, and a destination collision after replacing the
executable. Failed updates must preserve the old executable and database, remove
partially installed companion files, report the expected failure, and leave the old
version bootable. A failed installation keeps its retry marker; the test archives
that marker before booting the restored version. A successful update must install
the exact target executable and companion file and preserve consent, theme, active
tab, window placement and a user-content sentinel.

Windows must restart the updated application through the updater. On Linux the
updater exits without restarting; the test runner then launches the updated version
manually. The restarted application's version is checked against the target's prior
startup-test result. Updater diagnostics and scenario completion markers are uploaded
with the startup diagnostics. No public releases or live installations are modified.

To run locally after publishing target and baseline versions and running target
startup checks:

```powershell
./scripts/Test-PublishedUpdater.ps1 -PublishDirectory artifacts/published-windows -BaselineDirectory artifacts/baseline-windows -VerifiedPublishResults artifacts/smoke-results -ResultsDirectory artifacts/updater-results
```

On Linux run this command with PowerShell under Xvfb, as for startup checks. Each
results directory must be new. Release publication is blocked if any scenario fails.

Rollback keeps an independent copy of each original file before moving the live
file aside. This preserves recovery data even when a mounted filesystem loses a
renamed running-image path. Backups are retained if restoration cannot complete.
