using Addons.Providers;
using Core.All.Enums.Versions;

namespace Tests.Unit;

public sealed class GrpInfoParsingTests
{
    [Fact]
    public void ParseGrpInfoTest()
    {
        const string text = """"
            // EDUKE32 ADDON COMPILATION v4.0 (by NightFright)
            // ATOMIC EDITION
            
            grpinfo
            {
                name       "1999/2000 TC"
                scriptname "scripts/2000tc.con"
                size       17147390
                crc        0x23F56C4E
                flags      16
                dependency DUKE15_CRC
            }

            grpinfo
            {
                name       "25th Century Duke"
                scriptname "scripts/25th_century.con" 
                defname    "add.def"
                size       1385747
                crc        0x42836014
                flags      16
            }

            grpinfo
            {
                name       "A.Dream Trilogy"
                size       28245809
                crc        0x01987700
                flags      16
            }
            """";

        File.WriteAllText("test.grpinfo", text);
        var result = GrpInfoProvider.Parse("test.grpinfo");

        Assert.Equal(3, result.Count);

        Assert.Equal("1999/2000 TC", result[0].Name);
        Assert.Equal("scripts/2000tc.con", result[0].MainCon);
        Assert.Null(result[0].AddDef);
        Assert.Equal(17147390, result[0].Size);

        Assert.Equal("25th Century Duke", result[1].Name);
        Assert.Equal("scripts/25th_century.con", result[1].MainCon);
        Assert.Equal("add.def", result[1].AddDef);
        Assert.Equal(1385747, result[1].Size);

        Assert.Equal("A.Dream Trilogy", result[2].Name);
        Assert.Null(result[2].MainCon);
        Assert.Null(result[2].AddDef);
        Assert.Equal(28245809, result[2].Size);
    }
}
