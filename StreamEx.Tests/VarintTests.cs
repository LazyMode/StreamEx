using System;
using System.IO;
using System.Linq;
using Xunit;

public class VarintTests
{
    static int CalcBufferSize(int bits)
    {
        if (bits == 0)
            return 1;
        else if (bits == 64)
            return 9;

        return (int)Math.Ceiling(bits / 7d);
    }

    [Theory]
    [InlineData(0, "00 ")]
    [InlineData(1, "01 ")]
    [InlineData(3, "03 ")]
    [InlineData(127, "7F ")]
    [InlineData(128, "80 01 ")]
    [InlineData(129, "81 01 ")]
    [InlineData(150, "96 01 ")]
    [InlineData(270, "8E 02 ")]
    [InlineData(300, "AC 02 ")]
    [InlineData(16383, "FF 7F ")]
    [InlineData(16384, "80 80 01 ")]
    [InlineData(16385, "81 80 01 ")]
    [InlineData(86942, "9E A7 05 ")]
    [InlineData(0x78FFFFF0uL, "F0 FF FF C7 07")]
    [InlineData(0xFFFFFFFFuL, "FF FF FF FF 0F")]
    public void ForSamples32(uint value, string hex)
    {
        var buffer = new byte[5];
        using (var ms = new MemoryStream(buffer))
        {
            ms.WriteUInt32via7(value);
            var s = string.Join(" ", buffer.Select(b => b.ToString("X2")));
            Assert.StartsWith(hex, s, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Theory]
    [InlineData(0, "00 ")]
    [InlineData(1, "01 ")]
    [InlineData(3, "03 ")]
    [InlineData(127, "7F ")]
    [InlineData(128, "80 01 ")]
    [InlineData(129, "81 01 ")]
    [InlineData(150, "96 01 ")]
    [InlineData(270, "8E 02 ")]
    [InlineData(300, "AC 02 ")]
    [InlineData(16383, "FF 7F ")]
    [InlineData(16384, "80 80 01 ")]
    [InlineData(16385, "81 80 01 ")]
    [InlineData(86942, "9E A7 05 ")]
    [InlineData(0x78FFFFFFFFFFFFF0uL, "F0 FF FF FF FF FF FF FF 78")]
    [InlineData(0xFFFFFFFFFFFFFFFFuL, "FF FF FF FF FF FF FF FF FF")]
    public void ForSamples64(ulong value, string hex)
    {
        var buffer = new byte[9];
        using(var ms =new MemoryStream(buffer))
        {
            ms.WriteUInt64via7(value);
            var s = string.Join(" ", buffer.Select(b => b.ToString("X2")));
            Assert.StartsWith(hex, s, StringComparison.OrdinalIgnoreCase);
        }
    }

    public void ForBuffer32(uint integer, int count)
    {
        var buffer = new byte[count];
        using (var ms = new MemoryStream(buffer))
        {
            ms.WriteUInt32via7(integer);
            Assert.Equal(count, ms.Position);

            ms.Position = 0;

            var r = ms.ReadUInt32via7();
            Assert.Equal(integer, r);
        }
    }

    [Theory]
    [InlineData(32, 5)]
    [InlineData(29, 5)]
    [InlineData(28, 4)]
    [InlineData(22, 4)]
    [InlineData(21, 3)]
    [InlineData(15, 3)]
    [InlineData(14, 2)]
    [InlineData(8, 2)]
    [InlineData(7, 1)]
    [InlineData(1, 1)]
    [InlineData(0, 1)]
    public void ForArgs32(int takenBits, int count)
    {
        Assert.Equal(count, CalcBufferSize(takenBits));

        uint integer = 1;
        if (takenBits == 0)
            integer = 0;
        else
            integer <<= takenBits - 1;

        integer += (uint)(new Random().NextDouble() * integer);

        ForBuffer32(integer, count);
    }

    [Fact]
    public void For1to31bits()
    {
        for (var i = 1; i < 32; i++)
            ForArgs32(i, (int)Math.Ceiling(i / 7d));
    }

    public void ForBuffer64(ulong integer, int count)
    {
        var buffer = new byte[count];
        using (var ms = new MemoryStream(buffer))
        {
            ms.WriteUInt64via7(integer);
            Assert.Equal(count, ms.Position);

            ms.Position = 0;

            var r = ms.ReadUInt64via7();
            Assert.Equal(integer, r);
        }
    }

    [Theory]
    [InlineData(64, 9)]
    [InlineData(63, 9)]
    [InlineData(57, 9)]
    [InlineData(56, 8)]
    [InlineData(50, 8)]
    [InlineData(49, 7)]
    [InlineData(43, 7)]
    [InlineData(42, 6)]
    [InlineData(36, 6)]
    [InlineData(35, 5)]
    [InlineData(29, 5)]
    [InlineData(28, 4)]
    [InlineData(22, 4)]
    [InlineData(21, 3)]
    [InlineData(15, 3)]
    [InlineData(14, 2)]
    [InlineData(8, 2)]
    [InlineData(7, 1)]
    [InlineData(1, 1)]
    [InlineData(0, 1)]
    public void ForArgs64(int takenBits, int count)
    {
        Assert.Equal(count, CalcBufferSize(takenBits));

        ulong integer = 1;
        if (takenBits == 0)
            integer = 0;
        else
            integer <<= takenBits - 1;

        integer += (ulong)(new Random().NextDouble() * integer);

        ForBuffer64(integer, count);
    }

    [Fact]
    public void For1to63bits()
    {
        for (var i = 1; i < 64; i++)
            ForArgs64(i, (int)Math.Ceiling(i / 7d));
    }
}
