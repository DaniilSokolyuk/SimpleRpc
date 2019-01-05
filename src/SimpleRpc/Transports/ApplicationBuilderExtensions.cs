using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Transports.Abstractions.Server;

namespace SimpleRpc.Transports
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseSimpleRpcServer(this IApplicationBuilder builder)
        {
            builder.ApplicationServices.GetService<IServerTransport>().Configure(builder);

            return builder;
        }
    }
}
