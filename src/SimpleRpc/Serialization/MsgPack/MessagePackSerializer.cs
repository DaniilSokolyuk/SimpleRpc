using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using SimpleRpc.Serialization.Wire.Library.Extensions;

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
                new TypeFormatter<Type>(),
                new RpcRequestFormatter(),
                WireFallbackAnyObjectResolver.WireAnyObjectFormatter<Exception>.instance
            };

            if (TypeEx.TypeType != TypeEx.RuntimeType)
            {
                list.Add((IMessagePackFormatter)Activator.CreateInstance(typeof(TypeFormatter<>).MakeGenericType(TypeEx.RuntimeType)));
            }

            CompositeResolver.Register(list.ToArray());

            _resolver = CompositeResolver.Instance;
        }

        public string Name => Constants.DefaultSerializers.MessagePack;

        public string ContentType => "application/x-msgpack";

        public void Serialize(object message, Stream stream, Type type)
        {
            LZ4MessagePackSerializer.NonGeneric.Serialize(type, stream, message, _resolver);
        }

        public object Deserialize(Stream stream, Type type)
        {
            return LZ4MessagePackSerializer.NonGeneric.Deserialize(type, stream, _resolver);
        }
    }
}
