using MessagePack;
using MessagePack.Formatters;
using SimpleRpc.Serialization.Wire;

namespace SimpleRpc.Serialization.MsgPack
{
    public class WireFallbackAnyObjectResolver : IFormatterResolver
    {
        public static readonly WireFallbackAnyObjectResolver Instance = new WireFallbackAnyObjectResolver();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return WireAnyObjectFormatter<T>.instance;
        }

        public class WireAnyObjectFormatter<T> : IMessagePackFormatter<T>
        {
            public static WireAnyObjectFormatter<T> instance = new WireAnyObjectFormatter<T>();

            public int Serialize(ref byte[] bytes, int offset, T value, IFormatterResolver formatterResolver)
            {
                if (value == null)
                {
                    return MessagePackBinary.WriteNil(ref bytes, offset);
                }

                using (var stream = Utils.StreamManager.GetStream())
                {
                    WireMessageSerializer._serializer.Serialize(value, stream);
                    stream.Position = 0;

                    return Utils.CopyTo(stream, ref bytes, offset);
                }
            }

            T IMessagePackFormatter<T>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
            {
                if (MessagePackBinary.IsNil(bytes, offset))
                {
                    readSize = 1;
                    return default;
                }

                using (var stream = Utils.StreamManager.GetStream())
                {
                    var readedBytes = MessagePackBinary.ReadBytes(bytes, offset, out readSize);
                    
                    stream.Write(readedBytes, 0, readedBytes.Length);
                    stream.Position = 0;

                    return (T)WireMessageSerializer._serializer.Deserialize(stream);
                }
            }
        }
    }
}
