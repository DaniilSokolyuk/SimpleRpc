using MessagePack;
using Microsoft.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc
{
    internal static class Utils
    {
        public static RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        public static int CopyTo(Stream source, ref byte[] dstBytes, int dstOffset)
        {
            int written = 0;
            var buffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                while (true)
                {
                    int readLength = buffer.Length;
                    int read = source.Read(buffer, 0, readLength);

                    if (read == 0)
                    {
                        break;
                    }

                    written =+ MessagePackBinary.WriteBytes(ref dstBytes, dstOffset, buffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return written;
        }

        public static async Task CopyToDestAsync(Stream source, Stream destination, CancellationToken cancel)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                int read;
                while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    await destination.WriteAsync(buffer, 0, read, cancel).ConfigureAwait(false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static async Task CopyFromSourceAsync(Stream source, Stream destination, CancellationToken cancel)
        {
            var rentBuffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                int read;
                while ((read = await source.ReadAsync(rentBuffer, 0, rentBuffer.Length, cancel).ConfigureAwait(false)) > 0)
                {
                    destination.Write(rentBuffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentBuffer);
            }
        }
    }

}

