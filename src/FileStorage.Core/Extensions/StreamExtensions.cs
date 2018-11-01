using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileStorage.Core
{
    public static class StreamExtensions
    {
        public static byte[] ReadToEnd(this Stream stream)
        {
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static async Task<byte[]> ReadToEndAsync(this Stream stream, CancellationToken token)
        {
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length, token);
            return buffer;
        }
    }
}