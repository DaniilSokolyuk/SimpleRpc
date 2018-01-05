using System;
using System.Collections.Concurrent;
using Fasterflect;

namespace SimpleRpc.Transports.Abstractions.Client
{
    public class ClientConfiguration
    {
        private static readonly ConcurrentDictionary<string, IClientTransport> _clientTransports = new ConcurrentDictionary<string, IClientTransport>(StringComparer.OrdinalIgnoreCase);

        public static IClientTransport Get(string clientName)
        {
            if (!_clientTransports.TryGetValue(clientName, out var clientTransport))
            {
                throw new Exception($"Cant resolve client transport, make sure that you registered rpc client named {clientName}");
            }

            return clientTransport;
        }

        public static void Register<T>(string clientName, IClientTransportOptions<T> clientTransportOptions) where T : class, IClientTransport
        {
            if (!_clientTransports.TryAdd(clientName, (IClientTransport)typeof(T).CreateInstance(clientTransportOptions)))
            {
                throw new Exception($"Cant added client transport named {clientName}, maybe he's already registered");
            }
        }
    }
}
