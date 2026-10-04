---
name: buildlauncher-blood-art
description: Convert Build ART and Blood SURFACE.DAT to indexed PNG and DEF with the repository's .NET exporter, or verify an existing export with the Python checker. Use for ZIP-loaded resources, including Blood addons without RFF files.
---

# ART and Blood surface conversion

Select packaging through the [Blood profile](../../docs/games/blood.md); other
games must use their own profiles before applying this tool.

Run the .NET 10 exporter in `src/Addon.ArtExporter`. Read [the ART guide](../../docs/art-exporter.md) for commands, format limits, palette rules, output staging and the Python/Pillow verifier.

Check Blood addon inputs for RFF files case-insensitively, including nested archives. If present, the addon needs unpacking: preserve native ART/RFF/DAT and use [the RFF packaging skill](../buildlauncher-blood-rff/SKILL.md). No separate profile approval is needed. Do not run the ART exporter or drop VOXEL.DAT for that package; exporter size limits do not constrain native resources. Without RFF, ART/DAT require DEF-backed conversion; unsupported DAT formats remain an unresolved conversion step.

## Export contract

- Inspect root ART and optional SURFACE.DAT. The tool reads actual header ranges, preserves nonempty tile IDs and rejects malformed/overlapping resources. No fixed 256-slot assumption follows from filenames.
- Supply a verified target-game 256-color RGB ACT palette explicitly. No game palette is bundled. PNG indices rotate by one, source transparent index 255 becomes PNG index 0, and palette entries rotate correspondingly. Preserve decoded colors/alpha, not PNG compression bytes.
- Include the resulting DEF in the package manifest's def_modules and declare the TileFromTexture feature; resource export alone does not supply a manifest.
- Export into a new resource directory whose parent exists; keep input/output separate. The tool validates all resources before publishing a complete staged result. Copy validated DEF/tiles into a separate prepared addon package; preserve originals, credits and required resources.
- Signed offsets, animation type/range/speed and Blood extra/view bits are transferred. Backward animation ends at tile minus frame count. Unsupported metadata and empty-slot metadata cause errors rather than silent loss.
- SURFACE.DAT transfers values for exported tiles; zero omits surface. Missing DAT expects zero surfaces. A present table must cover exported IDs. Entries for absent/empty tiles are omitted and counted; the user permits absent-tile omissions. This is not full global DAT equivalence.
- Optional `--ifcrc` is disabled by default. Enable it only when the requested workflow needs source-pixel CRC conditions. The exporter closes texture blocks regardless of the flag.
- In the no-RFF Blood profile, VOXEL.DAT must eventually become a separate DEF. The user deferred this training to another example. The documented exporter rejects VOXEL input. Do not silently drop it or claim that voxel conversion exists.

The user excluded port compatibility checks. Report them as untested; resource validation does not establish a successful game launch. Use [the manifest skill](../buildlauncher-addon-json/SKILL.md) for DEF module references and package contracts.

## Verification

When changing the exporter, run the affected .NET tests and build. Its tests independently decode PNG chunks and exercise data fidelity and failure preservation.

For the simple reference grammar, optionally run `python .agents/skills/buildlauncher-blood-art/scripts/audit_art_profile.py SOURCE OUTPUT --def resources.def --palette GAME.act` with Python and Pillow. This helper is read-only and checks exact rotated PNG indices, palette, transparency, offsets, surfaces and forward/oscillating animations. It does not accept optional ifcrc, extra bits, backward animation or voxel syntax. The .NET exporter tests cover CRC, extra bits and backward animations.

Run its own behavioral checks with `python .agents/skills/buildlauncher-blood-art/scripts/test_audit_art_profile.py`. Exit 0 means these resource checks passed; manifests, final ZIP packaging and game execution require separate checks. Record tool version/options and hashes from conversion-report.json outside addon.json.
