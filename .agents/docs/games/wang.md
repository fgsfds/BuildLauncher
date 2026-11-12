# Shadow Warrior addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: Wang`; its recipes are not defaults for other games.

## Native resources and scripts

Keep native MAP/ART when the source and port use them. Shadow Warrior GRP
containers and port-specific DEF wrappers require their own role/signature audit;
there is no universal GRP mounting rule. Do not invent CON entry points: BL uses
GenericCampaign for Wang and does not transfer Duke main/module CON fields into
it. Blood RFF/INI declarations and Blood DAT conversion do not apply.

Inspect SW.DEF/other wrappers and active definetexture or tilefromtexture
references; rename roots reserved by the developer editor. Preserve native
stacked-sector/portal effects; they do not imply Duke binary-v9 TROR. The verified
version-7 map profile checks sector/wall/sprite bounds and exact native length.
Map variants may share one ZIP and distinct root manifests; follow the common
variant rules and use exact file-based startmap paths.

## VoidSW archived-map launch

On current `master`, the inherited file-based map builder supplies the full
archive path to VoidSW's `-g` switch. A reported VoidSW r10664 lookup failure was
reproduced with an absolute ZIP path. The fix is preserved in local commit
`93f640ea714e48bbb6f7b3f64c0c327170e1fdfe`. It adds the package directory to
the search path, passes the ZIP basename to `-g` and retains the selected map
path, for example `-j"./maps" -g"example.zip" -map "EXAMPLE.MAP"`.

Keep native maps and exact nested paths. Check which builder the tested
launcher uses and report root/nested map lookup separately from gameplay.

## Artwork

### Grid

Favor red/black with white or gold accents. Use martial-arts imagery, a silhouette, an emblem or an author illustration when it fits the source. Existing Shadow Warrior lettering and the yin-yang motif are common anchors. Twin Dragon, Wanton Destruction, Park and The Last Warrior show both detailed and simple covers.

### Title lettering

Use the original Shadow Warrior logotype or suitable author lettering as a
reference. Favor tall, very condensed uppercase glyphs with strong vertical
strokes, sharp flared or tapered terminals, and subtly curved lower ends. Render
with hard pixel edges and short color ramps. White or silver suits cool scenes;
warm ivory or muted bronze with a dark oxblood or black shadow suits brick,
dusk and warm lighting. Keep the narrow strokes readable at the final size.
