# Linux support and filesystem behavior

BuildLauncher targets Windows and Linux. Linux release smoke tests run the real
self-contained executable under Xvfb; unit and headless UI tests also run on Linux.
See [published release checks](published-build-tests.md) for startup and updater
coverage, including manual launch after a Linux update.

## Paths and data

The app remains portable: its database, logs, installed ports, addons and saves live
beside the executable. Use an installation directory writable by the current user.
This change does not migrate existing installations into XDG configuration/data
folders or make a system-owned installation directory writable.

Offline JSON files are resolved from an adjacent `db/` directory first. Development
builds can find the repository's `db/` by locating an ancestor containing
`BuildLauncher.slnx`; discovery does not assume a fixed build-output depth or take
files from unrelated ancestor directories.

Native absolute addon paths preserve literal backslashes. Game-folder cache keys
and addon-folder deduplication use case-sensitive comparisons on Linux. Existing
addon directories with dots in their names are recognized as directories. Windows
paths retained in copied settings must still be changed to valid Linux paths.
Legacy game resource filenames are not automatically renamed or case-folded.

Steam discovery recognizes the standard data directory, an absolute
`XDG_DATA_HOME`, `.steam/steam`, `.steam/root`, and the Flatpak data layout. It prefers
an installation with a library index and retains the default game library even if
that index is absent or unreadable. This does not grant access through a sandbox
or discover every possible custom Steam installation.

## Installation and launch

Addon executable checks use the current operating system's manifest section and
fall back to normal feature requirements when that section is absent. Installers
reject release metadata for another OS before downloading or changing files. Linux
port installation adds owner-execute permission to the validated main executable,
including releases whose archive did not preserve that permission.

Linux downloads and launch support still depend on each concrete port/tool and its
release metadata. A Windows-only port is not made Linux-compatible by path fixes;
for example, BuildGDX currently has no configured Linux executable. The tests do
not establish compatibility for every game, source port, distribution or desktop.

Directory replacement deliberately rejects symbolic links in existing installations.
Tests verify that staging does not follow them or delete their targets. Supporting
linked installations needs an explicit preservation policy; bypassing this check
can modify files outside the installation.

## Credentials

Encrypted credential storage currently uses Windows DPAPI. Linux settings disable
GitHub-token and S3-secret editing and explain the limitation. Reading credentials
from a copied Windows database returns unavailable values without throwing, while
preserving the encrypted database rows. Nonempty credential writes on Linux remain
unsupported; credentials are never silently stored as plaintext.

## Regression coverage

Tests cover published/development JSON lookup, paths with spaces, case-distinct game
folders, dotted addon directories, literal Linux backslashes, Steam layouts, foreign
OS releases, symlink staging rejection, unavailable encrypted credentials, and the
settings UI's platform capability. The Linux installer test extracts and executes a
small shell fixture to verify actual executable permissions; it does not launch a
real source port or game.
