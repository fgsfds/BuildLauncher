# NAM addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: NAM`; shared CON/GRP-family recipes apply alongside this profile.

## Shared preparation rules

Apply [the shared CON/GRP family guide](con-grp-family.md), including the rules
originally learned from Duke campaigns and large TCs. Use this game's manifest
name, scripts, base resources and supported port commands; keep its artwork below.

## Resource and runtime boundaries

NAM uses the same GRP container and CON script formats as Duke3D, Redneck and
WWII GI, and BL uses the Duke-style campaign model for its CON fields. Reuse
container inspection and CON include/reference checks; commands, definitions,
base data and port support remain game-specific. Inspect source
requirements and active scripts; do not substitute Duke scripts or declare
EDuke32_CON merely because CON files exist. Blood RFF/INI/DAT recipes do not apply.

## Artwork

### Grid

The reviewed library had no same-game grid/preview references. Use supplied author artwork or an actual addon scene and its existing title/logo. Do not label a borrowed Duke/Blood design as their established library style. Extend this style profile with observed references only after inspecting suitable same-game examples.

### Preview

Use a readable author title card, actual scene or resource showcase, with the exact game's palette. Record the absence of references and the chosen fallback.

Use the [common artwork rules](../addon-artwork-style.md) for dimensions, logo
placement, pixel handling, preservation and exact preview palette verification.
