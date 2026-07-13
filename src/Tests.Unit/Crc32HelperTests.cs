using Core.Client.Helpers;

namespace Tests.Unit;

public sealed class Crc32HelperTests : IDisposable
{
    private readonly string _tempFile;

    public Crc32HelperTests()
    {
        _tempFile = Path.GetTempFileName();
    }

    public void Dispose()
    {
        try { File.Delete(_tempFile); } catch { }
    }

    [Fact]
    public void GetCrc32_ReturnsNonNegativeLong()
    {
        File.WriteAllText(_tempFile, "test data for crc32");
        var crc = Crc32Helper.GetCrc32(_tempFile);
        Assert.True(crc >= 0);
    }

    [Fact]
    public void GetCrc32_SameContent_ReturnsSameHash()
    {
        File.WriteAllText(_tempFile, "hello world");
        var crc1 = Crc32Helper.GetCrc32(_tempFile);
        var crc2 = Crc32Helper.GetCrc32(_tempFile);
        Assert.Equal(crc1, crc2);
    }

    [Fact]
    public void GetCrc32_DifferentContent_ReturnsDifferentHash()
    {
        var file2 = Path.GetTempFileName();
        try
        {
            File.WriteAllText(_tempFile, "hello world");
            File.WriteAllText(file2, "different data");

            var crc1 = Crc32Helper.GetCrc32(_tempFile);
            var crc2 = Crc32Helper.GetCrc32(file2);
            Assert.NotEqual(crc1, crc2);
        }
        finally
        {
            try { File.Delete(file2); } catch { }
        }
    }

    [Fact]
    public void GetCrc32Hex_StartsWith0x()
    {
        File.WriteAllText(_tempFile, "test");
        var hex = Crc32Helper.GetCrc32Hex(_tempFile);
        Assert.StartsWith("0x", hex);
        Assert.Equal(10, hex.Length); // 0x + 8 hex chars
    }

    [Fact]
    public void GetCrc32_EmptyFile_ReturnsHash()
    {
        File.WriteAllText(_tempFile, "");
        var crc = Crc32Helper.GetCrc32(_tempFile);
        Assert.True(crc >= 0);
    }
}
