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
                var rpcRequest = (RpcRequest)null;
                var rpcError = (RpcError)null;
                var serializer = (IMessageSerializer)null;
                var result = (object)null;

                try
                {
                    serializer = SerializationHelper.GetByContentType(context.Request.ContentType);
                }
                catch (Exception e)
                {
                    rpcError = new RpcError
                    {
                        Code = 100,
                        Message = "Not supported ContentType",
                        Data = context.Request.ContentType
                    };

                    _logger.LogError(e, "Not supported ContentType", context.Request.ContentType);
                }

                if (serializer != null)
                {
                    try
                    {
                        rpcRequest = (RpcRequest)serializer.Deserialize(context.Request.Body, typeof(RpcRequest));
                    }
                    catch (Exception e)
                    {
                        rpcError = new RpcError
                        {
                            Code = 101,
                            Message = "Error on RpcRequest deserialization",
                            Data = e
                        };

                        _logger.LogError(e, "Error on RpcRequest deserialization");
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
                            Code = 102,
                            Message = "Error on RPC method invokation",
                            Data = e
                        };

                        _logger.LogError(e, "Error on RPC method invokation", rpcRequest);
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
            else
            {
                await _next(context);
            }
        }
    }
}
