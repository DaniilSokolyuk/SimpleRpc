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
    public static class SimpleRpcUtils
    {
        public static RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        public static async Task CopyToAsync(Stream source, Stream destination, CancellationToken cancel)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                while (true)
                {
                    cancel.ThrowIfCancellationRequested();

                    int read = 0;
                    if (source is MemoryStream)
                    {
                        read = source.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        read = await source.ReadAsync(buffer, 0, buffer.Length, cancel).ConfigureAwait(false);
                    }

                    // End of the source stream.
                    if (read == 0)
                    {
                        return;
                    }

                    cancel.ThrowIfCancellationRequested();

                    if (destination is MemoryStream)
                    {
                        destination.Write(buffer, 0, read);
                    }
                    else
                    {
                        await destination.WriteAsync(buffer, 0, read, cancel).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

}

