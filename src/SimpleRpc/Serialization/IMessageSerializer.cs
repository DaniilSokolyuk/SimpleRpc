using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization
{
    public interface IMessageSerializer
    {
        string Name { get; }

        string ContentType { get; }

        Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default);

        ValueTask<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default);
    }
}
