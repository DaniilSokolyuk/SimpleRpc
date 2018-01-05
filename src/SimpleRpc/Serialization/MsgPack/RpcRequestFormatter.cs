using MessagePack;
using MessagePack.Formatters;

namespace SimpleRpc.Serialization.MsgPack
{
    internal class RpcRequestFormatter : IMessagePackFormatter<RpcRequest>
    {
        public int Serialize(ref byte[] bytes, int offset, RpcRequest value, IFormatterResolver formatterResolver)
        {
            var startOffset = offset;

            offset += formatterResolver.GetFormatter<MethodModel>().Serialize(ref bytes, offset, value.Method, formatterResolver);
            offset += formatterResolver.GetFormatter<object[]>().Serialize(ref bytes, offset, value.Parameters, formatterResolver);

            return offset - startOffset;
        }

        public RpcRequest Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            var startOffset = offset;

            var methodModel = formatterResolver.GetFormatter<MethodModel>().Deserialize(bytes, offset, formatterResolver, out readSize);
            offset += readSize;

            var objectArray = formatterResolver.GetFormatter<object[]>().Deserialize(bytes, offset, formatterResolver, out readSize);
            offset += readSize;

            readSize = offset - startOffset;

            return new RpcRequest
            {
                Method = methodModel,
                Parameters = objectArray
            };
        }
    }
}
