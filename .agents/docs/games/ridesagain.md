# Redneck Rampage Rides Again addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: RidesAgain`; shared CON/GRP-family recipes apply alongside this profile.

## Shared preparation rules

Apply [the shared CON/GRP family guide](con-grp-family.md), including the rules
originally learned from Duke campaigns and large TCs. Use this game's manifest
name, scripts, base resources and supported port commands; keep its artwork below.

## Resource and runtime boundaries

Rides Again shares the GRP container and CON script formats with Redneck,
Duke3D, NAM and WWII GI. Reuse container inspection and CON include/reference
checks, with distinct game/edition and port
selection. Preserve a confirmed existing manifest identity and verify the current
Redneck-family factory/provider mapping before authoring a new campaign. Inspect
Rides Again-specific scripts, resources and builder branches; neither a Redneck
base-game package nor a Duke recipe establishes compatibility. No Blood RFF/INI.

## Artwork

### Grid

The reviewed library had no same-game grid/preview references. Use supplied author artwork or an actual addon scene and its existing title/logo. Do not label a borrowed Duke/Blood design as their established library style. Extend this style profile with observed references only after inspecting suitable same-game examples.

### Preview

Use a readable author title card, actual scene or resource showcase, with the exact game's palette. Record the absence of references and the chosen fallback.

Use the [common artwork rules](../addon-artwork-style.md) for dimensions, logo
placement, pixel handling, preservation and exact preview palette verification.
