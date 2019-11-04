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

        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            byte[] buff = null;

            try
            {
                var buffLength = _serializer.Value.Serialize(message, ref buff, 0);

                var bufferForLZ4 = ArrayPool<byte>.Shared.Rent(LZ4Codec.MaximumOutputSize(buffLength) + 8); //8 for headers
                try
                {
                    var encodedLength = LZ4Codec.Encode(
                        buff, 0, buffLength,
                        bufferForLZ4, 5, bufferForLZ4.Length - 5);

                    await stream.WriteAsync(bufferForLZ4, 0, encodedLength + 5).ConfigureAwait(false);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bufferForLZ4);
                }

            }
            finally
            {
                if (buff != null)
                {
                    CerasBufferPool.Pool.Return(buff);
                }
            }
        }

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            using (var pooledStream = SimpleRpcUtils.StreamManager.GetStream())
            {
                await SimpleRpcUtils.CopyToAsync(stream, pooledStream, cancellationToken).ConfigureAwait(false);

                pooledStream.Position = 0;

                var buffer = ArrayPool<byte>.Shared.Rent(pooledStream.Capacity);
                try
                {
                    pooledStream.Read(buffer, 0, pooledStream.Capacity);

                    return _serializer.Value.Deserialize<object>(buffer);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}
