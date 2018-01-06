using System;
using Fasterflect;
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
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            if (clientTransportOptions == null)
            {
                throw new ArgumentNullException(nameof(clientTransportOptions));
            }

            services.TryAddSingleton<IClientConfigurationManager, ClientConfigurationManager>();
            services.AddSingleton(new ClientConfiguration
            {
                Name = clientName,
                Transport = (IClientTransport)typeof(T).CreateInstance(clientTransportOptions)
            });

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy<T>(this IServiceCollection services, string clientName)
        {
            AddSimpleRpcProxy(services, typeof(T), clientName);

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy(this IServiceCollection services, Type interfaceToProxy, string clientName)
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            if (interfaceToProxy == null)
            {
                throw new ArgumentNullException(nameof(interfaceToProxy));
            }

            if (!interfaceToProxy.IsInterface)
            {
                throw new NotSupportedException("You can use AddSimpleRpcProxy only on interfaces");
            }

            services.TryAddSingleton(interfaceToProxy, sp => sp.GetService<IClientConfigurationManager>().Get(clientName).BuildProxy(interfaceToProxy));

            return services;
        }

        public static IServiceCollection AddSimpleRpcServer<T>(
            this IServiceCollection services,
            IServerTransportOptions<T> serverTransportOptions) where T : class, IServerTransport, new()
        {
            if (serverTransportOptions == null)
            {
                throw new ArgumentNullException(nameof(serverTransportOptions));
            }

            var serverTransport = new T();

            services.AddSingleton<IServerTransport>(serverTransport);
            serverTransport.ConfigureServices(services, serverTransportOptions);

            return services;
        }
    }
}
