using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hyperion;
using Microsoft.IO;

namespace SimpleRpc.Serialization.Hyperion
{
    public class HyperionMessageSerializer : IMessageSerializer
    {
        private static RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        internal static Serializer _serializer;

        static HyperionMessageSerializer()
        {
            _serializer = new Serializer();
        }

        public string ContentType => "application/x-hyperion";

        public async Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default)
        {
            using (var pooledStream = StreamManager.GetStream())
            {
                _serializer.Serialize(message, pooledStream);

                pooledStream.Position = 0;

                await SimpleRpcUtils.CopyToAsync(pooledStream, stream, cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default)
        {
            if (stream is MemoryStream ms)
            {
                return _serializer.Deserialize(ms);
            }

            using (var pooledStream = StreamManager.GetStream())
            {
                await SimpleRpcUtils.CopyToAsync(stream, pooledStream, cancellationToken).ConfigureAwait(false);

                pooledStream.Position = 0;

                return _serializer.Deserialize(pooledStream);
            }
        }
    }
}
