using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleRpc.Serialization
{
    internal interface ISerializationHelper
    {
        IMessageSerializer TryGetByTypeName(string contentType);

        IMessageSerializer GetByContentType(string contentType);
    }

    internal class SerializationHelper : ISerializationHelper
    {
        private readonly IDictionary<string, IMessageSerializer> _contentTypeSerializer;
        private readonly IDictionary<string, IMessageSerializer> _typeNameSerializer;

        public SerializationHelper(IEnumerable<IMessageSerializer> messageSerializers)
        {
            _contentTypeSerializer = messageSerializers.ToDictionary(x => x.ContentType);
            _typeNameSerializer = messageSerializers.ToDictionary(x => x.GetType().Name, StringComparer.OrdinalIgnoreCase);
        }

        public IMessageSerializer TryGetByTypeName(string typeName)
        {
            if (typeName != null && _typeNameSerializer.TryGetValue(typeName, out var serializer))
            {
                return serializer;
            }

            return _typeNameSerializer[typeof(Ceras.CerasMessageSerializer).Name];
        }

        public IMessageSerializer GetByContentType(string contentType)
        {
            _contentTypeSerializer.TryGetValue(contentType, out var serializer);
            return serializer;
        }
    }
}
