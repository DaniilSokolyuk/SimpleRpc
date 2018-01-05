using System.Collections.Generic;
using SimpleRpc.Serialization.MsgPack;
using SimpleRpc.Serialization.Wire;

namespace SimpleRpc.Serialization
{
    public static class SerializationHelper
    {
        private static readonly IDictionary<string, IMessageSerializer> _serializers;

        static SerializationHelper()
        {
            _serializers = new Dictionary<string, IMessageSerializer>
            {
                [Constants.ContentTypes.Wire] = new WireMessageSerializer(),
                [Constants.ContentTypes.MessagePack] = new MsgPackSerializer(),
            };
        }

        public static IMessageSerializer GetSerializer(string contentType)
        {
            return _serializers[contentType];
        }
    }
}
