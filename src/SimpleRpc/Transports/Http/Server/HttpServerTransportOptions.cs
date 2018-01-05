using SimpleRpc.Transports.Abstractions.Server;

namespace SimpleRpc.Transports.Http.Server
{
    public class HttpServerTransportOptions : IServerTransportOptions<HttpServerTransport>
    {
        public string Path { get; set; }
    }
}
