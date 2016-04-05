using System;
using System.IO;

static partial class StreamEx
{
    public static int ReadBlock(this Stream source, byte[] buffer, int offset, int count)
    {
        var len = buffer.Length;
        if (offset < 0 || offset > len)
            throw new ArgumentException(null, nameof(offset));
        if (count < 0 || count > len)
            throw new ArgumentException(null, nameof(count));
        if (count == 0)
            return 0;
        if (offset + count > len)
            throw new ArgumentException();

        var read = 0;
        do
        {
            var a = source.Read(buffer, offset + read, count - read);
            if (a == 0)
                break;

            read += a;
        } while (read < count);

        return read;
    }

    public static void WriteUInt64via7(this Stream target, ulong value)
    {
        for (var i = 0; i < 8; i++)
        {
            if (value < 128)
                break;

            target.WriteByte((byte)(value & 0x7F | 0x80));
            value >>= 7;
        }
        target.WriteByte((byte)value);
    }
    public static void WriteSInt64via7(this Stream target, long value)
        => target.WriteUInt64via7(value.Zig());

    public static void WriteUInt32via7(this Stream target, uint value)
    {
        for (var i = 0; i < 4; i++)
        {
            if (value < 128)
                break;

            target.WriteByte((byte)(value & 0x7F | 0x80));
            value >>= 7;
        }
        target.WriteByte((byte)value);
    }
    public static void WriteSInt32via7(this Stream target, int value)
        => target.WriteUInt32via7(value.Zig());

    public static ulong ReadUInt64via7(this Stream source)
    {
        int b;
        ulong tmp = 0;

        for (var s = 0; s < 56; s += 7)
        {
            b = source.ReadByte();
            if (b < 0)
                throw new EndOfStreamException();

            if (b < 128)
                return tmp | (ulong)b << s;

            tmp |= ((ulong)(b & 0x7F) << s);
        }

        b = source.ReadByte();
        if (b < 0)
            throw new EndOfStreamException();

        return tmp | (ulong)b << 56;
    }
    public static long ReadSInt64via7(this Stream source)
        => source.ReadUInt64via7().Zag();

    public static uint ReadUInt32via7(this Stream source)
    {
        int b;
        uint tmp = 0;

        for (var s = 0; s < 28; s += 7)
        {
            b = source.ReadByte();
            if (b < 0)
                throw new EndOfStreamException();

            if (b < 128)
                return tmp | (uint)b << s;

            tmp |= ((uint)(b & 0x7F) << s);
        }

        b = source.ReadByte();
        if (b < 0)
            throw new EndOfStreamException();

        return tmp | (uint)b << 28;
    }
    public static int ReadSInt32via7(this Stream source)
        => source.ReadUInt32via7().Zag();
}
