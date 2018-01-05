using System;
using System.IO;
using MessagePack;
using SimpleRpc.Serialization.Wire.Library;

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

        public void Serialize(object message, Stream stream, Type type)
        {
            using (var memStream = new MemoryStream())
            {
                _serializer.Serialize(message, memStream);

                memStream.TryGetBuffer(out var buffer);

                var lz4Buffer = LZ4MessagePackSerializer.ToLZ4Binary(buffer);

                stream.Write(lz4Buffer, 0, lz4Buffer.Length);
            }
        }

        public object Deserialize(Stream stream, Type type)
        {
            var bytes = LZ4MessagePackSerializer.Decode(stream);

            using (var memStream = new MemoryStream(bytes))
            {
                return _serializer.Deserialize(memStream);
            }
        }
    }
}
