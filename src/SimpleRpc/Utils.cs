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

        public static async Task CopyToAsync(Stream source, Stream destination, long? count, int bufferSize, CancellationToken cancel)
        {
            long? bytesRemaining = count;

            var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                while (true)
                {
                    // The natural end of the range.
                    if (bytesRemaining.HasValue && bytesRemaining.GetValueOrDefault() <= 0)
                    {
                        return;
                    }

                    cancel.ThrowIfCancellationRequested();

                    int readLength = buffer.Length;
                    if (bytesRemaining.HasValue)
                    {
                        readLength = (int)Math.Min(bytesRemaining.GetValueOrDefault(), (long)readLength);
                    }
                    int read = source.Read(buffer, 0, readLength);

                    if (bytesRemaining.HasValue)
                    {
                        bytesRemaining -= read;
                    }

                    // End of the source stream.
                    if (read == 0)
                    {
                        return;
                    }

                    cancel.ThrowIfCancellationRequested();

                    await destination.WriteAsync(buffer, 0, read, cancel).ConfigureAwait(false);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

}

