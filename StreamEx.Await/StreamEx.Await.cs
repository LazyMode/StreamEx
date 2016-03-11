using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

static partial class StreamEx
{
    public static async Task<int> ReadBlockAsync(this Stream source, byte[] buffer, int offset, int count, CancellationToken token)
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
            var a = await source.ReadAsync(buffer, offset + read, count - read, token);
            if (a == 0)
                break;

            read += a;
        } while (read < count);

        return read;
    }
    public static Task<int> ReadBlockAsync(this Stream source, byte[] buffer, int offset, int count)
        => source.ReadBlockAsync(buffer, offset, count, CancellationToken.None);

    #region ReadByteAsync

    public static async Task<bool> ReadByteAsync(this Stream source, byte[] buffer, int offset, CancellationToken token)
    {
        var r = await source.ReadAsync(buffer, offset, 1, token);
        return r == 1;
    }

    public static async Task<bool> ReadByteAsync(this Stream source, byte[] buffer, CancellationToken token)
    {
        if (buffer.Length != 1)
            throw new ArgumentException(null, nameof(buffer));

        var r = await source.ReadAsync(buffer, 0, 1, token);
        return r == 1;
    }

    #endregion

    public static async Task<byte> ReadByteFailFastAsync(this Stream source, byte[] buffer, int offset, CancellationToken token)
    {
        if (!await source.ReadByteAsync(buffer, offset, token))
            throw new EndOfStreamException();

        return buffer[offset];
    }

    public static async Task<byte> ReadByteFailFastAsync(this Stream source, byte[] buffer, CancellationToken token)
    {
        if (buffer == null)
            buffer = new byte[1];

        if (!await source.ReadByteAsync(buffer, token))
            throw new EndOfStreamException();

        return buffer[0];
    }

    public static async Task<ulong> ReadUInt64via7async(this Stream source, CancellationToken token)
    {
        ulong tmp = 0;
        var buffer = new byte[1];

        for (var s = 0; s < 56; s += 7)
        {
            var b = await source.ReadByteFailFastAsync(buffer, token);

            if (b < 128)
                return tmp | (ulong)b << s;

            tmp |= ((ulong)(b & 0x7F) << s);
        }

        return tmp | (ulong)await source.ReadByteFailFastAsync(buffer, token) << 56;
    }
    public static async Task<long> ReadSInt64via7async(this Stream source, CancellationToken token)
        => (await source.ReadUInt64via7async(token)).Zag();

    public static async Task<uint> ReadUInt32via7async(this Stream source, CancellationToken token)
    {
        uint tmp = 0;
        var buffer = new byte[1];

        for (var s = 0; s < 28; s += 7)
        {
            var b = await source.ReadByteFailFastAsync(buffer, token);

            if (b < 128)
                return tmp | (uint)b << s;

            tmp |= ((uint)(b & 0x7F) << s);
        }

        return tmp | (uint)await source.ReadByteFailFastAsync(buffer, token) << 28;
    }
    public static Task<uint> ReadUInt32via7async(this Stream source)
        => source.ReadUInt32via7async(CancellationToken.None);
    public static async Task<int> ReadSInt32via7async(this Stream source, CancellationToken token)
        => (await source.ReadUInt32via7async(token)).Zag();
    public static Task<int> ReadSInt32via7async(this Stream source)
        => source.ReadSInt32via7async(CancellationToken.None);
}
