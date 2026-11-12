# Duke Nukem 3D addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: Duke3D`; shared CON/GRP-family recipes apply alongside this profile.

## Resource boundaries

CON main/modules and native GRP/ART are appropriate candidates for this family
when the selected port supports them. Verify archive signatures and each port's
switches; ZIP delivery is not evidence that nested GRPs auto-mount. Do not declare
Blood INI/RFF entry points or interpret Blood SURFACE.DAT/VOXEL.DAT as Duke metadata.
Duke, Redneck, NAM and WWII GI use the shared preparation rules below; choose
the actual game's commands, edition and palette when applying them.

## Duke3D CON campaigns

Follow [the shared CON/GRP family rules](con-grp-family.md#con-campaigns).
Use `game.name: Duke3D` and the required Duke edition. Duke-specific examples
in that guide cover the 1.3D base script, reserved duke3d.def, EDuke32 launch/menu
commands and TROR feature detection.

## Large Duke3D total conversions

Follow [the shared large-TC rules](con-grp-family.md#large-total-conversions)
and select actual Duke resources and port features.

## Artwork

### Grid

The common layout is a full-height addon/map screenshot with a centered title near the top, often in one to three lines. Use wide, angular block capitals in bright red with a dark outline/shadow, as in Roch Series, ADG Episode and Red Series. Keep the setting visible and the title readable. Existing author covers and gold/yellow Duke logos are alternatives when already part of the addon, as in Alien Armageddon or Legacy Edition.


## Reviewed-library resource evidence

The 2026-10-09 read-only audit confirmed TROR sector flags in 65 Duke maps across
ten packages whose manifests omitted TROR. Inspect actual active maps and flags
rather than inheriting those historical feature lists. This evidence did not
certify launch or renderer compatibility.
