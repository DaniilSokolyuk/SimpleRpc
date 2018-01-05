using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Transports.Abstractions.Server;

namespace SimpleRpc.Transports.Http.Server
{
    public class HttpServerTransport : IServerTransport
    {
        public void ConfigureServices<T>(IServiceCollection services, IServerTransportOptions<T> serverTransportOptions) where T : class, IServerTransport, new()
        {
            if (serverTransportOptions.GetType() != typeof(HttpServerTransportOptions))
            {
                throw new ArgumentException("Options has not supported type");
            }

            services.AddSingleton(serverTransportOptions.GetType(), sp => serverTransportOptions);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<HttpTransportMidleware>();
        }
    }
}
