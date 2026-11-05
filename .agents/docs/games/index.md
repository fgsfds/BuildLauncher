# Game-specific addon preparation profiles

Read the [common preparation plan](../addon-conversion-plan.md) and
[common artwork guide](../addon-artwork-style.md), then the actual game's profile.
Each profile owns resource/script recipes and its artwork style. The common
artwork type rule applies to all profiles: TCs/campaigns and standalone games
need grid and preview; maps and mods need only preview. ZIP is a delivery
container, not proof that a port supports every embedded resource format.

## Format boundaries

| Game | Profile | Script/container candidates to verify | Artwork |
| --- | --- | --- | --- |
| Duke3D | [Duke Nukem 3D](duke3d.md) | Duke CON, native GRP/ART, DEF | [Duke style](duke3d.md#artwork) |
| Blood | [Blood](blood.md) | Blood INI, RFF/RFS, Blood DAT, ART/DEF profiles | [Blood style](blood.md#artwork) |
| Wang | [Shadow Warrior](wang.md) | Native MAP/ART/GRP, port DEF; no Duke CON contract | [Wang style](wang.md#artwork) |
| Fury | [Ion Fury](fury.md) | Fury CON, native GRP/ART, DEF | [Fury style](fury.md#artwork) |
| Redneck | [Redneck Rampage](redneck.md) | Redneck CON, native GRP/ART, port DEF | [Redneck style](redneck.md#artwork) |
| RidesAgain | [Rides Again](ridesagain.md) | Redneck-family CON/GRP; verify edition routing | [Rides Again style](ridesagain.md#artwork) |
| NAM | [NAM](nam.md) | NAM CON, native GRP/ART, port DEF | [NAM style](nam.md#artwork) |
| WW2GI | [WWII GI](ww2gi.md) | Shared CON/GRP formats; check game-specific payloads | [WWII GI style](ww2gi.md#artwork) |
| Slave | [Exhumed / PowerSlave](slave.md) | Native maps/resources, STUFF.DAT, port DEF; no Duke CON contract | [Slave style](slave.md#artwork) |
| Standalone | [Standalone](standalone.md) | Project-specific engine, scripts and containers | [Project branding](standalone.md#artwork) |
| Other game enums | [Unestablished profiles](other-games.md) | Research required; enum recognition is insufficient | Verified game branding/palette |

- RFF and rff_main/rff_sound are Blood-only. The parser/extraction trigger accepting
  a field elsewhere does not make it a valid other-game resource declaration.
- Duke3D, Redneck, Rides Again, NAM and WWII GI share the GRP container
  and CON script formats. Apply [the shared CON/GRP family preparation rules](con-grp-family.md)
  to them, including campaign/large-TC rules originally learned from Duke. Game-specific commands, definitions, base resources and
  launch support still belong to the selected profile.
- CON main/module support is limited to the game/port/script dialect. BL's
  DukeCampaign factory branches cover Duke3D, Fury, Redneck, NAM and WW2GI;
  inspect Redneck-family edition routing separately. GenericCampaign branches
  do not transfer those campaign CON fields. This is not a promise that every
  port implements every additional-CON switch.
- GRP support is game- and port-dependent. Verify a container's signature, payload
  role and mounting command. Neither a .grp suffix nor a renamed .dat proves its
  format; resource archives, base data and selection markers have different roles.
- DEF language/features, native maps and DAT metadata are also game/port-specific.
  Blood metadata conversion and Duke TROR rules are not universal Build recipes.
- Do not inherit another game's typography, imagery or palette. Use the selected
  profile and actual references; profiles with limited evidence label fallbacks.

Reviewed implementation entry points: [addon factory](../../../src/Addons/Addons/AddonFactory.cs),
[command builders](../../../src/Ports/Builders/CmdParametersBuilder.cs),
[VoidSW builder](../../../src/Ports/Builders/VoidSWCmdParametersBuilder.cs) and
[game enums](../../../src/Core.All/Enums/GameEnum.cs). Recheck changed code when
using this table; generation guidance does not certify port/game compatibility.
