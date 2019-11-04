using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRpc.Serialization;
using SimpleRpc.Serialization.Ceras;
using SimpleRpc.Transports.Abstractions.Client;
using SimpleRpc.Transports.Abstractions.Server;
using SimpleRpc.Transports.Http.Client;
using SimpleRpc.Transports.Http.Server;

namespace SimpleRpc.Transports
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleRpcClient(
            this IServiceCollection services,
            string clientName,
            HttpClientTransportOptions options,
            Action<IHttpClientBuilder> httpclientBuilder = null)
        {
            if (string.IsNullOrEmpty(clientName))
            {
                throw new ArgumentNullException(nameof(clientName));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var clientBuilder = services.AddHttpClient(
                clientName,
                client =>
                {
                    var url = new Uri(options.Url);
                    client.BaseAddress = url;

                    if (options.DefaultRequestHeaders != null)
                    {
                        foreach (var header in options.DefaultRequestHeaders)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

                    client.DefaultRequestHeaders.Add(Constants.Other.ApplicationName, options.ApplicationName);
                    client.DefaultRequestHeaders.ConnectionClose = false;
                    client.DefaultRequestHeaders.Host = url.Host;
                });
            httpclientBuilder?.Invoke(clientBuilder);

            services.TryAddSingleton<IClientConfigurationManager, ClientConfigurationManager>();
            services.TryAddSingleton<ISerializationHelper, SerializationHelper>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMessageSerializer), typeof(CerasMessageSerializer)));
            services.AddSingleton(
                sp => new ClientConfiguration
                {
                    Name = clientName,
                    Transport = new HttpClientTransport(
                        clientName,
                        sp.GetRequiredService<ISerializationHelper>().TryGetByTypeName(options.Serializer),
                        sp.GetRequiredService<ISerializationHelper>(),
                        sp.GetRequiredService<IHttpClientFactory>())
                });

            return services;
        }

        public static IServiceCollection AddSimpleRpcProxy<T>(this IServiceCollection services, string clientName) => AddSimpleRpcProxy(services, typeof(T), clientName);

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

        public static IServiceCollection AddSimpleRpcServer(this IServiceCollection services, HttpServerTransportOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var serverTransport = new HttpServerTransport();

            services.TryAddSingleton<IServerTransport>(serverTransport);
            services.TryAddSingleton<ISerializationHelper, SerializationHelper>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMessageSerializer), typeof(CerasMessageSerializer)));
            serverTransport.ConfigureServices(services, options);

            return services;
        }
    }
}
