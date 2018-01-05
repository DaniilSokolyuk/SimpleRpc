using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using SimpleRpc.Serialization.Wire.Library.Extensions;
using SimpleRpc.Serialization.Wire.Library.Internal;

namespace SimpleRpc.Serialization.MsgPack
{
    public class TypeFormatter<T> : IMessagePackFormatter<T> where T : Type
    {
        private static readonly ConcurrentDictionary<ByteArrayKey, Type> _byteTypeNameLookup = new ConcurrentDictionary<ByteArrayKey, Type>(ByteArrayKeyComparer.Instance);
        private static readonly ConcurrentDictionary<Type, byte[]> _typeByteNameLookup = new ConcurrentDictionary<Type, byte[]>();

        public static readonly List<KeyValuePair<string, string>> _macroses = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("System", "$s"),
            new KeyValuePair<string, string>("Collection", "$c"),
        };

        public int Serialize(ref byte[] bytes, int offset, T value, IFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                return MessagePackBinary.WriteNil(ref bytes, offset);
            }

            var stringAsBytes = _typeByteNameLookup.GetOrAdd(value, type =>
            {
                var shortName = type.GetShortAssemblyQualifiedName();
                _macroses.ForEach(x=>
                {
                    shortName = shortName.Replace(x.Key, x.Value);
                });

                var byteArr =new ByteArrayKey(MessagePackBinary.GetEncodedStringBytes(shortName));

                _byteTypeNameLookup.TryAdd(byteArr, type); //add to reverse cache

                return byteArr.Bytes;
            });

            return MessagePackBinary.WriteBytes(ref bytes, offset, stringAsBytes);
        }

        T IMessagePackFormatter<T>.Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            if (MessagePackBinary.IsNil(bytes, offset))
            {
                readSize = 1;
                return null;
            }

            var byteArr = new ByteArrayKey(MessagePackBinary.ReadBytes(bytes, offset, out readSize));

            return (T)_byteTypeNameLookup.GetOrAdd(byteArr, b =>
            {
                var typename = TypeEx.ToQualifiedAssemblyName(MessagePackBinary.ReadString(byteArr.Bytes, 0, out _));
                _macroses.ForEach(x =>
                {
                    typename = typename.Replace(x.Value, x.Key);
                });

                var type = Type.GetType(typename, true);

                _typeByteNameLookup.TryAdd(type, b.Bytes); //add to reverse cache

                return type;
            });
        }
    }
}
