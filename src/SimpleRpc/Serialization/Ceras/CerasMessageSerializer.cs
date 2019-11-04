using Ceras;
using K4os.Compression.LZ4;
using SimpleRpc.Serialization;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization.Ceras
{
    public class CerasMessageSerializer : IMessageSerializer
    {
        public static Type Type = typeof(CerasMessageSerializer);

        private class CerasBuffer : ICerasBufferPool
        {
            public byte[] RentBuffer(int minimumSize)
            {
                return ArrayPool<byte>.Shared.Rent(Math.Max(minimumSize, 65536));
            }

            public void Return(byte[] buffer)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private SerializerConfig config = null;
        private readonly ThreadLocal<CerasSerializer> _serializer;

        public CerasMessageSerializer()
        {
            CerasBufferPool.Pool = new CerasBuffer();

            _serializer = new ThreadLocal<CerasSerializer>(() =>
            {
                config = new SerializerConfig();

                config.Warnings.ExceptionOnStructWithAutoProperties = false;

                config.OnConfigNewType = (t) =>
                {
                    t.TargetMembers = TargetMember.AllPublic;

                    if (typeof(Exception).IsAssignableFrom(t.Type))
                    {
                        t.TargetMembers = TargetMember.All;
                    }

                    //t.TypeConstruction = TypeConstruction.ByConstructor()
                };

                config.ConfigType<MethodModel>().ConstructBy(() => new MethodModel(null, null, null, null));
                return new CerasSerializer(config);
            });

        }

        public string ContentType => "application/x-ceras";

        private static readonly byte Header = 0x62; //98
        private static int HeaderLength = 9;

        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            byte[] uncromressedBuff = null;

            try
            {
                var uncromressedLength = _serializer.Value.Serialize(message, ref uncromressedBuff, 0);

                var compressedBuff = ArrayPool<byte>.Shared.Rent(LZ4Codec.MaximumOutputSize(uncromressedLength) + HeaderLength); //9 for headers
                try
                {
                    var compressedLength = LZ4Codec.Encode(
                        uncromressedBuff, 0, uncromressedLength,
                        compressedBuff, HeaderLength, compressedBuff.Length - HeaderLength);

                    var offset = 0;
                    SerializerBinary.WriteByte(ref compressedBuff, ref offset, Header);
                    SerializerBinary.WriteInt32Fixed(ref compressedBuff, ref offset, compressedLength);
                    SerializerBinary.WriteInt32Fixed(ref compressedBuff, ref offset, uncromressedLength);

                    await stream.WriteAsync(compressedBuff, 0, compressedLength + HeaderLength).ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(compressedBuff);
                }

            }
            finally
            {
                if (uncromressedBuff != null)
                {
                    CerasBufferPool.Pool.Return(uncromressedBuff);
                }
            }
        }

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            var lengthBuffer = new byte[9];
            await stream.ReadAsync(lengthBuffer, 0, HeaderLength, cancellationToken);

            var offset = 0;
            var header = SerializerBinary.ReadByte(lengthBuffer, ref offset);
            if (header != Header)
                throw new Exception("Not expected header error");

            var compressedLength = SerializerBinary.ReadInt32Fixed(lengthBuffer, ref offset);
            var uncompressedLength = SerializerBinary.ReadInt32Fixed(lengthBuffer, ref offset);

            var buffer = ArrayPool<byte>.Shared.Rent(compressedLength + uncompressedLength);
            try
            {
                await stream.ReadAsync(buffer, offset, compressedLength).ConfigureAwait(false);
                offset += compressedLength; // = HeaderLength + compressedLength

                LZ4Codec.Decode(
                        buffer, HeaderLength, compressedLength,
                        buffer, offset, uncompressedLength);

                object obj = null;

                _serializer.Value.Deserialize(ref obj, buffer, ref offset);

                return obj;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
