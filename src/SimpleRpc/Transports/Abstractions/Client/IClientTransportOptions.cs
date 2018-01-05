namespace SimpleRpc.Transports.Abstractions.Client
{
    public interface IClientTransportOptions<T> where T : class, IClientTransport
    {
        string ApplicationName { get; }

        string Serializer { get; }
    }
}
