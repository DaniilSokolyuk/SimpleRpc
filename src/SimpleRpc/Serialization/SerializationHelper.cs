using System;
using System.Collections.Generic;
using SimpleRpc.Serialization.MsgPack;
using SimpleRpc.Serialization.Wire;

namespace SimpleRpc.Serialization
{
    public static class SerializationHelper
    {
        private static readonly IDictionary<string, string> contentTypesToNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly IDictionary<string, IMessageSerializer> _serializers = new Dictionary<string, IMessageSerializer>(StringComparer.OrdinalIgnoreCase);

        static SerializationHelper()
        {
            Add(new WireMessageSerializer());
            Add(new MsgPackSerializer());
        }

        public static IMessageSerializer Add<T>(IMessageSerializer serializer) where T : class, IMessageSerializer
        {
            return _serializers[contentType];
        }

        public static IMessageSerializer GetByName(string name)
        {
            return _serializers[contentType];
        }

        public static IMessageSerializer GetByContentType(string contentType)
        {
            return _serializers[contentType];
        }
    }
}
