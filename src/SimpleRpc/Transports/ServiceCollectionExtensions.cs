using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        public static IServiceCollection AddSimpleRpcProxy<T>(this IServiceCollection services, string clientName)
        {
            AddSimpleRpcProxy(services, typeof(T), clientName);

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy(this IServiceCollection services, Type interfaceToProxy, string clientName)
        {
            if (!interfaceToProxy.IsInterface)
            {
                throw new NotSupportedException("You can use AddSimpleRpcProxy only on interfaces");
            }

            services.TryAddSingleton(interfaceToProxy, sp => ClientConfiguration.Get(clientName).BuildProxy(interfaceToProxy));

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
