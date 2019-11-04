using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace SimpleRpc.Serialization.MsgPack
{
    public class MsgPackSerializer : IMessageSerializer
    {
        internal static IFormatterResolver _resolver;

        static MsgPackSerializer()
        {
            CompositeResolver.Register(TypelessContractlessStandardResolver.Instance, FallbackAnyObjectResolver.Instance);

            var list = new List<IMessagePackFormatter>
            {
                new RpcRequestFormatter(),
                WireFallbackAnyObjectResolver.WireAnyObjectFormatter<Exception>.instance
            };

            CompositeResolver.Register(list.ToArray());

            _resolver = CompositeResolver.Instance;
        }

        public string Name => Constants.DefaultSerializers.MessagePack;

        public string ContentType => "application/x-msgpack";


        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            using (var pooledStream = Utils.StreamManager.GetStream())
            {
                MessagePackSerializer.NonGeneric.Serialize(type, pooledStream, message, _resolver);

                pooledStream.Position = 0;

                await Utils.CopyToDestAsync(pooledStream, stream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            if (stream is MemoryStream ms)
            {
                return MessagePackSerializer.NonGeneric.Deserialize(type, ms, _resolver);
            }

            using (var pooledStream = Utils.StreamManager.GetStream())
            {
                await Utils.CopyFromSourceAsync(stream, pooledStream, cancellationToken).ConfigureAwait(false);

                pooledStream.Position = 0;

                return MessagePackSerializer.NonGeneric.Deserialize(type, pooledStream, _resolver);
            }
        }
    }
}
