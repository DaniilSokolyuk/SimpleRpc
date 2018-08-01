using System;
using System.IO;
using LZ4;
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
            using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Compress, LZ4StreamFlags.IsolateInnerStream))
            {
                _serializer.Serialize(message, lz4Stream);
            }
        }

        public object Deserialize(Stream stream, Type type)
        {
            using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
            {
                return _serializer.Deserialize(lz4Stream);
            }
        }
    }
}
