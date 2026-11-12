# Exhumed / PowerSlave addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: Slave`; its recipes are not defaults for other games.

## Resource and runtime boundaries

BL uses GenericCampaign for this game: do not declare Duke CON entry points.
Inspect native maps, DEF/resources and the selected port. Base-game detection
requires STUFF.DAT; verify its actual format rather than assuming DAT is Blood
metadata or blindly labeling it GRP. Any native container recipe needs source and
port evidence. Blood RFF/INI and Duke episode-menu recipes do not apply.

## Artwork

### Grid

The single Return to Ruins cover uses an Egyptian-adventure illustration, warm orange/gold/red and prominent PowerSlave/addon lettering. This is a possible starting point when the source has that setting, not proof that every addon needs the same imagery.
