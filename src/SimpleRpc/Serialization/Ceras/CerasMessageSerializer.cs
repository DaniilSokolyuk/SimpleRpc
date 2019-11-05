using Ceras;
using K4os.Compression.LZ4;
using SimpleRpc.Serialization;
using System;
using System.Buffers;
using System.Threading;

namespace SimpleRpc.Serialization.Ceras
{
    public sealed class CerasMessageSerializer : BaseLZ4CompressSerializer
    {
        private struct CerasMemoryOwner : IMemoryOwner<byte>
        {
            byte[] _underlayingArray;
            int _length;

            public CerasMemoryOwner(byte[] array, int length)
            {
                _underlayingArray = array;
                _length = length;
            }

            public Memory<byte> Memory => new Memory<byte>(_underlayingArray, 0, _length);

            public void Dispose()
            {
                CerasBufferPool.Pool.Return(_underlayingArray);
            }
        }

        private class CerasBuffer : ICerasBufferPool
        {
            public byte[] RentBuffer(int minimumSize)
            {
                return ArrayPool<byte>.Shared.Rent(Math.Max(minimumSize, 65536));
            }

            public void Return(byte[] buffer)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private SerializerConfig config = null;
        private readonly ThreadLocal<CerasSerializer> _serializer;

        public CerasMessageSerializer()
        {
            CerasBufferPool.Pool = new CerasBuffer();

            _serializer = new ThreadLocal<CerasSerializer>(() =>
            {
                config = new SerializerConfig();

                config.Warnings.ExceptionOnStructWithAutoProperties = false;

                config.OnConfigNewType = (t) =>
                {
                    t.TargetMembers = TargetMember.AllPublic;

                    t.TypeConstruction = TypeConstruction.ByUninitialized();

                    if (typeof(Exception).IsAssignableFrom(t.Type))
                    {
                        t.TargetMembers = TargetMember.All;
                    }
                };

                return new CerasSerializer(config);
            });

        }

        public override string ContentType => "application/x-ceras";


        public override IMemoryOwner<byte> SerializeCore(object message, Type type)
        {
            byte[] uncromressedBuff = null;

            var uncromressedLength = _serializer.Value.Serialize(message, ref uncromressedBuff, 0);

            return new CerasMemoryOwner(uncromressedBuff, uncromressedLength);
        }

        public override object DeserializeCore(byte[] buffer, int offset, int uncompressedLength)
        {
            object obj = null;

            _serializer.Value.Deserialize(ref obj, buffer, ref offset, uncompressedLength);

            return obj;
        }
    }
}
