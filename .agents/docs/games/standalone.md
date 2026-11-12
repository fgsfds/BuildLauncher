# Standalone addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: Standalone`; its recipes are not defaults for other games.

## Resource and runtime boundaries

Standalone is a project identity, not a universal engine dialect. Establish its
required engine/executable, resource formats, scripts, palette and working
directory from the project. BL constructs GenericCampaign here; a model field
or a standalone exemption in the developer editor does not establish CON,
RFF or GRP runtime support. Preserve verified custom engines instead of omitting
them as redundant ports. Never select a palette from the Standalone label alone.

## Artwork

### Grid

Follow the project's own branding and art direction. AMC Squad uses a blue science-fiction ensemble, Electric Highways a minimal dark geometric design, and WGRealms Demon Throne an orange fantasy-action cover. Do not apply a single inherited Duke/Blood template just because a project uses Build.

### Palette

Use the actual project palette. Standalone is a category, not a shared palette.
