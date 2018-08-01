using System;
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

        public HttpTransportMidleware(
            RequestDelegate next, 
            ILogger<HttpTransportMidleware> logger, 
            IServiceProvider serviceProvider, 
            HttpServerTransportOptions httpServerTransportOptions)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _httpServerTransportOptions = httpServerTransportOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != _httpServerTransportOptions.Path)
            {
                await _next(context);
            }
            else
            {
                var rpcRequest = (RpcRequest)null;
                var rpcError = (RpcError)null;
                var result = (object)null;
                var serializer = SerializationHelper.GetByContentType(context.Request.ContentType);

                if (serializer == null)
                {
                    rpcError = new RpcError { Code = RpcErrorCode.NotSupportedContentType };
                    _logger.LogError(rpcError.Code.ToString(), context.Request.ContentType);
                }
                else
                {
                    try
                    {
                        rpcRequest = (RpcRequest)serializer.Deserialize(context.Request.Body, typeof(RpcRequest));
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = RpcErrorCode.IncorrectRequestBodyFormat,
                            Exception = e,
                        };

                        _logger.LogError(e, rpcError.Code.ToString());
                    }
                }

                if (rpcRequest != null)
                {
                    try
                    {
                        result = await rpcRequest.Invoke(_serviceProvider);
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = RpcErrorCode.RemoteMethodInvocation,
                            Exception = e,
                        };

                        _logger.LogError(e, rpcError.Code.ToString(), rpcRequest);
                    }
                }

                if (rpcError != null)
                {
                    serializer = SerializationHelper.GetByName(Constants.DefaultSerializers.MessagePack);
                }

                context.Response.ContentType = serializer.ContentType;
                serializer.Serialize(
                    new RpcResponse
                    {
                        Result = result,
                        Error = rpcError
                    },
                    context.Response.Body,
                    typeof(RpcResponse));
            }
        }
    }
}
