using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleRpc.Serialization
{
    public interface IMessageSerializer
    {
        string ContentType { get; }

        Task SerializeAsync(Stream stream, object message, Type type, CancellationToken cancellationToken = default);

        Task<object> DeserializeAsync(Stream stream, Type type, CancellationToken cancellationToken = default);
    }
}
