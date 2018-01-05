using Microsoft.AspNetCore.Builder;
using SimpleRpc.Transports.Abstractions.Server;
using Microsoft.Extensions.DependencyInjection;

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
