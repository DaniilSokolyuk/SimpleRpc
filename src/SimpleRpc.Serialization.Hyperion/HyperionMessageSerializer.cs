using System;
using System.Buffers;
using Hyperion;
using Microsoft.IO;

namespace SimpleRpc.Serialization.Hyperion
{
    public sealed class HyperionMessageSerializer : BaseLZ4CompressSerializer
    {
        private struct HyperionMemoryOwner : IMemoryOwner<byte>
        {
            byte[] _underlayingArray;
            int _length;

            public HyperionMemoryOwner(byte[] array, int length)
            {
                _underlayingArray = array;
                _length = length;
            }

            public Memory<byte> Memory => new Memory<byte>(_underlayingArray, 0, _length);

            public void Dispose()
            {
                ArrayPool<byte>.Shared.Return(_underlayingArray);
            }
        }

        private static RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();

        internal static Serializer _serializer;

        static HyperionMessageSerializer()
        {
            _serializer = new Serializer();
        }

        public override string ContentType =>  "application/x-hyperion";


        public override IMemoryOwner<byte> SerializeCore(object message, Type type)
        {
            using (var pooledStream = StreamManager.GetStream())
            {
                _serializer.Serialize(message, pooledStream);

                var length = (int)pooledStream.Position;

                var rentBuffer = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    pooledStream.Position = 0;
                    pooledStream.Read(rentBuffer, 0, length);

                    return new HyperionMemoryOwner(rentBuffer, length);
                }
                catch
                {
                    ArrayPool<byte>.Shared.Return(rentBuffer);
                    throw;
                }
            }
        }

        public override object DeserializeCore(byte[] buffer, int offset, int count)
        {
            using (var pooledStream = StreamManager.GetStream())
            {
                pooledStream.Write(buffer, offset, count);

                pooledStream.Position = 0;

                return _serializer.Deserialize(pooledStream);
            }
        }
    }
}
