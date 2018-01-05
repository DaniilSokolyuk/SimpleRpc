using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Transports.Abstractions.Client;
using SimpleRpc.Transports.Abstractions.Server;

namespace SimpleRpc.Transports
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleRpcClient<T>(
            this IServiceCollection services,
            string clientName,
            IClientTransportOptions<T> clientTransportOptions) where T : class, IClientTransport
        {
            ClientConfiguration.Register(clientName, clientTransportOptions);

            return services;
        }

        public static IServiceCollection AddSimpleRpcServer<T>(
            this IServiceCollection services,
            IServerTransportOptions<T> serverTransportOptions) where T : class, IServerTransport, new()
        {
            var serverTransport = new T();

            services.AddSingleton<IServerTransport>(serverTransport);
            serverTransport.ConfigureServices(services, serverTransportOptions);

            return services;
        }
    }
}
