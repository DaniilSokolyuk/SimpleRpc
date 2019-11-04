﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRpc.Serialization;
using SimpleRpc.Serialization.Hyperion;

namespace SimpleRpc.Hyperion
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSimpleRpcHyperionSerializer(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IMessageSerializer), typeof(HyperionMessageSerializer)));
            return services;
        }
    }
}
