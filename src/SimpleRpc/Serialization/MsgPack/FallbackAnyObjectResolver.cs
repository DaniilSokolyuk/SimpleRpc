using MessagePack;
using MessagePack.Formatters;

namespace SimpleRpc.Serialization.MsgPack
{
    public class FallbackAnyObjectResolver : IFormatterResolver
    {
        public static readonly FallbackAnyObjectResolver Instance = new FallbackAnyObjectResolver();

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return TypelessGenericObjectFormatter<T>.instance;
        }

        public class TypelessGenericObjectFormatter<T> : IMessagePackFormatter<T>
        {
            public static TypelessGenericObjectFormatter<T> instance = new TypelessGenericObjectFormatter<T>();

            public int Serialize(ref byte[] bytes, int offset, T value, IFormatterResolver formatterResolver)
            {
                return TypelessFormatter.Instance.Serialize(ref bytes, offset, value, formatterResolver);
            }

            T IMessagePackFormatter<T>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
            {
                return (T)TypelessFormatter.Instance.Deserialize(bytes, offset, formatterResolver, out readSize);
            }
        }
    }
}
