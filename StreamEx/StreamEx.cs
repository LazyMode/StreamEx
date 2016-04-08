using System;
using System.IO;
using System.Text;

#if EXPOSE_EVERYTHING || EXPOSE_STREAMEX
public
#endif
static class StreamEx
{
    private static readonly char[] LineEnds = new[] { '\n', '\r' };

    public static char? ReadProtocolLineWithEnd(this Stream source, StringBuilder sb, bool skipFirstLineFeed = false)
    {
        var b = source.ReadByte();
        if (skipFirstLineFeed && b == '\n')
            b = source.ReadByte();

        for (; b >= 0; b = source.ReadByte())
        {
            var c = (char)b;
            sb.Append(c);

            switch (c)
            {
                case '\n':
                    return '\n';
                case '\r':
                    if (source.CanSeek)
                    {
                        var pos = source.Position;
                        b = source.ReadByte();
                        if (b >= 0)
                        {
                            c = (char)b;
                            if (c == '\n')
                            {
                                sb.Append('\n');
                                return '\n';
                            }
                            source.Position = pos;
                        }
                    }
                    return '\r';
            }
        }

        return null;
    }

    public static string ReadProtocolLineWithEnd(this Stream source, bool skipFirstLineFeed = false)
    {
        var sb = new StringBuilder();
        source.ReadProtocolLineWithEnd(sb, skipFirstLineFeed);
        return sb.ToString();
    }

    public static char? ReadProtocolLine(this Stream source, StringBuilder sb, bool skipFirstLineFeed = false)
    {
        var b = source.ReadByte();
        if (skipFirstLineFeed && b == '\n')
            b = source.ReadByte();

        for (; b >= 0; b = source.ReadByte())
        {
            var c = (char)b;

            switch (c)
            {
                case '\n':
                    return '\n';
                case '\r':
                    if (source.CanSeek)
                    {
                        var pos = source.Position;
                        b = source.ReadByte();
                        if (b >= 0)
                        {
                            c = (char)b;
                            if (c == '\n')
                                return '\n';
                            source.Position = pos;
                        }
                    }
                    return '\r';
            }

            sb.Append(c);
        }

        return null;
    }

    public static string ReadProtocolLine(this Stream source, bool skipFirstLineFeed = false)
    {
        var sb = new StringBuilder();
        source.ReadProtocolLine(sb, skipFirstLineFeed);
        return sb.ToString();
    }

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
