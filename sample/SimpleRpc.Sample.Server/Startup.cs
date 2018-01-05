using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Sample.Shared;
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRPC.Sample.Server;

namespace SimpleRpc.Sample.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<IFooService, FooServiceImpl>();

            services.AddSimpleRpcServer(new HttpServerTransportOptions {Path = "/rpc"});
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseSimpleRpcServer();
        }
    }
}
