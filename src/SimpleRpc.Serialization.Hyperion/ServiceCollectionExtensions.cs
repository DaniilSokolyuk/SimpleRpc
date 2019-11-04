using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleRpc.Serialization;

namespace SimpleRpc.Serialization.Hyperion
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
