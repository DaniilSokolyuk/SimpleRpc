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
        private readonly HttpServerTransportOptions _httpServerTransportOptions;
        private readonly ISerializationHelper _serializationHelper;

        public HttpTransportMidleware(
            RequestDelegate next,
            ILogger<HttpTransportMidleware> logger,
            HttpServerTransportOptions httpServerTransportOptions,
            ISerializationHelper serializationHelper)
        {
            _next = next;
            _logger = logger;
            _httpServerTransportOptions = httpServerTransportOptions;
            _serializationHelper = serializationHelper;
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
                var deserializer = _serializationHelper.GetByContentType(context.Request.ContentType);

                if (deserializer == null)
                {
                    rpcError = new RpcError { Code = RpcErrorCode.NotSupportedContentType };
                    _logger.LogError(rpcError.Code.ToString(), context.Request.ContentType);
                }
                else
                {
                    try
                    {
                        rpcRequest = (RpcRequest)await deserializer.DeserializeAsync(context.Request.Body, typeof(RpcRequest));
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
                        result = await rpcRequest.Invoke(context.RequestServices);
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


                var serializer = deserializer ?? _serializationHelper.TryGetByTypeName(null);

                context.Response.ContentType = serializer.ContentType;
                await serializer.SerializeAsync(
                    context.Response.Body,
                    new RpcResponse
                    {
                        Result = result,
                        Error = rpcError
                    },
                    typeof(RpcResponse));
            }
        }
    }
}
