using Ceras;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization.Wire
{
    public class CerasMessageSerializer : IMessageSerializer
    {
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
        private ThreadLocal<CerasSerializer> _serializer;

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

        public string Name => Constants.DefaultSerializers.Ceras;

        public string ContentType => "application/x-ceras";

        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            byte[] buff = null;

            try
            {
                var length = _serializer.Value.Serialize(message, ref buff, 0);

                if (stream is MemoryStream)
                {
                    stream.Write(buff, 0, length);
                }
                else
                {
                    await stream.WriteAsync(buff, 0, length).ConfigureAwait(false);
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
            if (stream is MemoryStream ms && ms.TryGetBuffer(out var msBuffer))
            {
                return _serializer.Value.Deserialize<object>(msBuffer.Array);
            }

            using (var pooledStream = Utils.StreamManager.GetStream())
            {
                await Utils.CopyToAsync(stream, pooledStream, cancellationToken).ConfigureAwait(false);

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
