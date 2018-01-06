using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRpc.Transports.Abstractions.Server
{
    public interface IServerTransport
    {
        void ConfigureServices<T>(IServiceCollection services, IServerTransportOptions<T> serverTransportOptions) where T : class, IServerTransport, new();

        void Configure(IApplicationBuilder app);
    }
}
