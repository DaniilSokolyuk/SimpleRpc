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
            var rentBuffer = ArrayPool<byte>.Shared.Rent(65536);
            try
            {
                var buffer = rentBuffer; //becauce buffer ref can be changed in serialize
                var len = MessagePackSerializer.NonGeneric.Serialize(type, ref buffer, 0, message, _resolver);

                // do not need resize.
                await stream.WriteAsync(buffer, 0, len, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentBuffer);
            }
        }

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
            {
                return MessagePackSerializer.NonGeneric.Deserialize(type, streamBuffer, _resolver);
            }

            var rentBuffer = ArrayPool<byte>.Shared.Rent(65536);
            var buf = rentBuffer; //becauce buf ref can be changed in FastResize
            try
            {
                int length = 0;
                int read;
                while ((read = await stream.ReadAsync(buf, length, buf.Length - length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    length += read;
                    if (length == buf.Length)
                    {
                        MessagePackBinary.FastResize(ref buf, length * 2);
                    }
                }

                return MessagePackSerializer.NonGeneric.Deserialize(type, buf, _resolver);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentBuffer);
            }
        }
    }
}
