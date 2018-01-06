namespace SimpleRpc.Transports.Abstractions.Client
{
    public class ClientConfiguration
    {
        public string Name { get; set; }
        
        public IClientTransport Transport { get; set; }
    }
}
