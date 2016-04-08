using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;

using Xunit;

public class ProtocolTets
{
    private static readonly char[] LineEnds = new[] { '\n', '\r' };

    [Theory]
    [InlineData("foo", new[] { "foo" })]
    [InlineData("foo\n", new[] { "foo\n" })]
    [InlineData("foo\r", new[] { "foo\r" })]
    [InlineData("foo\r\n", new[] { "foo\r\n" })]
    [InlineData("foo\nbar", new[] { "foo\n", "bar" })]
    [InlineData("foo\rbar", new[] { "foo\r", "bar" })]
    [InlineData("foo\r\nbar", new[] { "foo\r\n", "bar" })]
    public void ForSeekable(string input, params string[] lines)
    {
        using (var ms = new MemoryStream(Encoding.ASCII.GetBytes(input)))
        {
            foreach (var line in lines)
                Assert.Equal(line, ms.ReadProtocolLineWithEnd());

            ms.Position = 0;
            foreach (var line in lines)
                Assert.Equal(line.TrimEnd(LineEnds), ms.ReadProtocolLine());
        }
    }

    [Theory]
    [InlineData("foo", new[] { "foo" })]
    [InlineData("foo\n", new[] { "foo\n" })]
    [InlineData("foo\r", new[] { "foo\r" })]
    [InlineData("foo\r\n", new[] { "foo\r" })]
    [InlineData("foo\nbar", new[] { "foo\n", "bar" })]
    [InlineData("foo\rbar", new[] { "foo\r", "bar" })]
    [InlineData("foo\r\nbar", new[] { "foo\r", "bar" })]
    public void ForNonSeekable(string input, params string[] lines)
    {
        using (var s = new AnonymousPipeServerStream())
        using (var c = new AnonymousPipeClientStream(s.GetClientHandleAsString()))
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            s.Write(bytes, 0, bytes.Length);
            s.Close();

            var skipLF = false;
            foreach (var line in lines)
            {
                Assert.Equal(line, c.ReadProtocolLineWithEnd(skipLF));
                skipLF = (line.Last() == '\r');
            }
        }

        using (var s = new AnonymousPipeServerStream())
        using (var c = new AnonymousPipeClientStream(s.GetClientHandleAsString()))
        {
            var bytes = Encoding.ASCII.GetBytes(input);
            s.Write(bytes, 0, bytes.Length);
            s.Close();

            var skipLF = false;
            foreach (var line in lines)
            {
                Assert.Equal(line.TrimEnd(LineEnds), c.ReadProtocolLine(skipLF));
                skipLF = (line.Last() == '\r');
            }
        }
    }
}
