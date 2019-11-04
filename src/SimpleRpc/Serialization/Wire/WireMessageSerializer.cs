using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hyperion;

namespace SimpleRpc.Serialization.Wire
{
    public class WireMessageSerializer : IMessageSerializer
    {
        internal static Serializer _serializer;

        static WireMessageSerializer()
        {
            _serializer = new Serializer();
        }

        public string Name => Constants.DefaultSerializers.Wire;

        public string ContentType => "application/x-wire";

        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            using (var pooledStream = Utils.StreamManager.GetStream())
            {
                _serializer.Serialize(message, pooledStream);

                pooledStream.Position = 0;

                await Utils.CopyToAsync(pooledStream, stream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            if (stream is MemoryStream ms)
            {
                return _serializer.Deserialize(ms);
            }

            using (var pooledStream = Utils.StreamManager.GetStream())
            {
                await Utils.CopyToAsync(stream, pooledStream, cancellationToken).ConfigureAwait(false);

                pooledStream.Position = 0;

                return _serializer.Deserialize(pooledStream);            
            }
        }
    }
}
