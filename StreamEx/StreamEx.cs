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
}
