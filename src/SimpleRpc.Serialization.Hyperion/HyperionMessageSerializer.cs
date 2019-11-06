using System;
using System.Buffers;
using System.IO;
using Hyperion;

namespace SimpleRpc.Serialization.Hyperion
{
    public sealed class HyperionMessageSerializer : BaseLZ4CompressSerializer
    {
        private readonly Serializer _serializer;

        public HyperionMessageSerializer()
        {
            _serializer = new Serializer();
        }

        public override string ContentType => "application/x-hyperion";


        public override IMemoryOwner<byte> SerializeCore(object message, Type type)
        {
            var pooledStream = new PooledMemoryStream(ArrayPool<byte>.Shared, 65536);
            _serializer.Serialize(message, pooledStream);

            pooledStream.Position = 0;

            return pooledStream;
        }

        public override object DeserializeCore(byte[] buffer, int offset, int count)
        {
            using (var pooledStream = new MemoryStream(buffer, offset, count))
            {
                pooledStream.Position = 0;

                return _serializer.Deserialize(pooledStream);
            }
        }
    }
}
