using Ceras;
using K4os.Compression.LZ4;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization
{
    public abstract class BaseLZ4CompressSerializer : IMessageSerializer
    {
        public abstract string ContentType { get; }

        private static readonly byte Header = 0x62; //98
        private static int HeaderLength = 9;

        public abstract IMemoryOwner<byte> SerializeCore(object message, Type type);

        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            using (var serializedMemory = SerializeCore(message, type))
            {
                var uncompressed = serializedMemory.Memory;

                var compressedBuff = ArrayPool<byte>.Shared.Rent(LZ4Codec.MaximumOutputSize(uncompressed.Length) + HeaderLength);
                try
                {
                    //write body, skip header
                    var compressedLength = LZ4Codec.Encode(
                        uncompressed.Span, 
                        new Span<byte>(compressedBuff, HeaderLength, compressedBuff.Length - HeaderLength));

                    //write header
                    var offset = 0;
                    SerializerBinary.WriteByte(ref compressedBuff, ref offset, Header);
                    SerializerBinary.WriteInt32Fixed(ref compressedBuff, ref offset, compressedLength);
                    SerializerBinary.WriteInt32Fixed(ref compressedBuff, ref offset, uncompressed.Length);

                    await stream.WriteAsync(compressedBuff, 0, compressedLength + HeaderLength).ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(compressedBuff);
                }
            }
        }

        public abstract object DeserializeCore(byte[] buffer, int offset, int uncompressedLength);

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            var lengthBuffer = new byte[9];
            await stream.ReadAsync(lengthBuffer, 0, HeaderLength, cancellationToken);

            var offset = 0;
            var header = SerializerBinary.ReadByte(lengthBuffer, ref offset); 
            if (header != Header) throw new Exception("Not expected header error");
            var compressedLength = SerializerBinary.ReadInt32Fixed(lengthBuffer, ref offset);
            var uncompressedLength = SerializerBinary.ReadInt32Fixed(lengthBuffer, ref offset);

            var buffer = ArrayPool<byte>.Shared.Rent(compressedLength + uncompressedLength);
            try
            {
                await stream.ReadAsync(buffer, 0, compressedLength).ConfigureAwait(false);

                LZ4Codec.Decode(
                        buffer, 0, compressedLength,
                        buffer, compressedLength, uncompressedLength);

                object obj = DeserializeCore(buffer, compressedLength, uncompressedLength);

                return obj;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
