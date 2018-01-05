using System;

namespace SimpleRpc.Transports.Abstractions.Client
{
    public static class ClientProxyExtensions
    {
        public static T CreateRpcProxy<T>(this IServiceProvider sp, string clientName) where T : class
        {
            return (T) ClientConfiguration.Get(clientName).BuildProxy(typeof(T));
        }

        public static object CreateRpcProxy(this Type type, string clientName)
        {
            return ClientConfiguration.Get(clientName).BuildProxy(type);
        }
    }
}
