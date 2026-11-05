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
references; rename reserved roots when required by the validator. Preserve native
stacked-sector/portal effects; they do not imply Duke binary-v9 TROR. The verified
version-7 map profile checks sector/wall/sprite bounds and exact native length.
Map variants may share one ZIP and distinct root manifests; follow the common
variant rules and use exact file-based startmap paths.

## VoidSW archived-map launch

For VoidSW archived maps, add the package directory to the search path and load
its ZIP by basename before passing the selected map: `-j"./maps" -g"example.zip"
-map "EXAMPLE.MAP"`. A reported VoidSW r10664 startup failure was reproduced when
`-g` received an absolute ZIP path; the directory-plus-basename form mounted the
same archive and passed map lookup. This is an argument-generation issue, not a
reason to unpack or convert intact native maps. Keep nested map paths exact and
check both root and nested variants when investigating a launch failure. A startup
lookup check does not certify gameplay, multiplayer or other ports.


## Artwork

### Grid

Favor red/black with white or gold accents. Use martial-arts imagery, a silhouette, an emblem or an author illustration when it fits the source. Existing Shadow Warrior lettering and the yin-yang motif are common anchors. Twin Dragon, Wanton Destruction, Park and The Last Warrior show both detailed and simple covers.

### Preview

The observed Red Dragon Clan preview uses the same red silhouette background and centered white game/addon title as its grid. Treat this as a limited reference; use an author title card or game-accurate scene when a new addon has another identity.

Use the [common artwork rules](../addon-artwork-style.md) for dimensions, logo
placement, pixel handling, preservation and exact preview palette verification.

## Shadow Warrior title lettering

For newly created Shadow Warrior / Wang addon titles, prefer lettering inspired
by the original Shadow Warrior logotype: tall, very condensed uppercase glyphs,
strong vertical strokes, sharp flared or tapered terminals and subtly curved,
sweeping lower ends. Avoid the wide chunky block capitals commonly used for Duke
covers. Inspect and supply the original game logo or a suitable author logo as an
imagegen reference; describe the letterforms rather than claiming an unverified
font name. Retain the actual addon name and established author branding.

Render the title as coarse 8-bit pixel lettering, with hard stepped edges and
short color ramps; avoid smooth vector text, antialiasing and glossy gradients.
Match its colors to the scene: white/silver can suit cool backgrounds, while warm
ivory and muted bronze with a dark oxblood/black shadow suit brick, dusk and warm
lamps. Maintain readable contrast and narrow stroke width at final size.

Preserve the background's existing resolution, texture detail and colors.
Pixelate the logo independently; do not downsample the entire cover merely to
match its title. A 200x300 logical grid enlarged 3x is optional for new scenery,
not a requirement to reduce an approved detailed background. A grid may remain
RGB at 600x900 to retain that detail; the preview must still use the exact indexed
game palette at 320x200. Restore original background pixels outside the intended
edit area if generation alters them. Moving or recoloring a title must not change
buildings, composition, scenery, camera position or background pixel scale.

For grid title placement, center the visible logo within the upper third at
(300,150) on 600x900. For a preview containing a logo, center it at (160,100) on
320x200 and make it large and legible, with scenery visible around it. Preserve
logo proportions; use its visible bounds rather than canvas padding. Author
screenshots need no added heading unless requested, but any added logo follows
these placement rules. Inspect all required final images after sizing and preview palette
conversion.
