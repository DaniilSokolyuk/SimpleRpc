using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SimpleRpc.Serialization;

namespace SimpleRpc.Transports.Http.Server
{
    internal class HttpTransportMidleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpTransportMidleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpServerTransportOptions _httpServerTransportOptions;

        public HttpTransportMidleware(RequestDelegate next, ILogger<HttpTransportMidleware> logger, IServiceProvider serviceProvider, HttpServerTransportOptions httpServerTransportOptions)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpServerTransportOptions = httpServerTransportOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == _httpServerTransportOptions.Path)
            {
                try
                {
                    var serializer = SerializationHelper.GetByContentType(context.Request.ContentType);
                    context.Response.ContentType = serializer.ContentType;

                    var rpcRequest = (RpcRequest)serializer.Deserialize(context.Request.Body, typeof(RpcRequest));

                    var result = await rpcRequest.Invoke(_serviceProvider);
                    if (result != null)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                        serializer.Serialize(result, context.Response.Body, typeof(object));
                    }
                    else
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.NoContent;
                    }
                }
                catch (Exception ex)
                {
                    var serializer = SerializationHelper.GetByName(Constants.DefaultSerializers.MessagePack);
                    context.Response.ContentType = serializer.ContentType;

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    serializer.Serialize(ex, context.Response.Body, typeof(Exception));

                    _logger.LogError(ex, "Rpc process error");
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
