# WWII GI addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: WW2GI`; shared CON/GRP-family recipes apply alongside this profile.

## Shared preparation rules

Apply [the shared CON/GRP family guide](con-grp-family.md), including the rules
originally learned from Duke campaigns and large TCs. Use this game's manifest
name, scripts, base resources and supported port commands; keep its artwork below.

## Resource and runtime boundaries

WWII GI uses the same GRP container and CON script formats as Duke3D, Redneck
and NAM, and BL uses the Duke-style campaign model for its CON fields. Reuse
container inspection and CON include/reference checks. Inspect WWII GI
scripts, port support and the role/signature of native containers, including
base data with unusual extensions. The builder refers to PLATOONL.DAT/PLATOONL.DEF
for an official addon; this is not evidence that arbitrary DAT files are Blood
metadata or that every DEF suffix contains the same language. No Blood RFF/INI.

## Artwork

### Grid

No reviewed WWII GI artwork profile is established. Use verified author artwork
and inspect original WWII GI branding; record any fallback without calling it an
observed library style.

### Preview

Use the actual WWII GI palette and game-specific title/scene; do not borrow
Blood or Duke branding solely because the game uses Build.

Use the [common artwork rules](../addon-artwork-style.md) for dimensions, logo
placement, pixel handling, preservation and exact preview palette verification.
