using Common.Enums;
using Mods.Serializable;
using System.Text.Json;

namespace Tests;

public class Test
{
    private readonly string aaa = """
        {
            "id": "merlijn-shakygrounds",
            "game": "duke3d",
            "title": "Shaky Grounds",
            "author": "Merlijn v. Oostrum",
            "version": "4.0",
            "description": { "path" : "sg_addondesc.txt" },
            "preview": "preview.png",
            "CON": { "type": "module", "path": "./sgmodule.con" },
            "gamecrc": [ "0xdeadbeef", -1742012854 ],
            "DEF":
            [
              { "type": "main", "path": "path/to/moduleA.def"},
              { "type": "module", "path": "path/to/moduleB.def"}
            ],
            "RTS": "path/to/game.rts",
            "dependencies":
            [
              {"id": "dukevaca" },
              {"id": "vacaplus", "version": ">=1.2.2" }
            ],
            "incompatibles": {"id": "brutal-duke", "version": "<2.0" },
            "startmap": { "file": "path/to/file.map" }
        }
""";


    private readonly string bb = """
        {
            "id": "merlijn-shakygrounds",
            "game": "duke3d",
            "title": "Shaky Grounds"
        }
""";

    [Fact]
    public void Test1()
    {
        var result = JsonSerializer.Deserialize(bb, AddonManifestContext.Default.AddonDto);
    }
}









