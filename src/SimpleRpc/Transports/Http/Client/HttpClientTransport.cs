using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SimpleRpc.Serialization;
using SimpleRpc.Transports.Abstractions.Client;

namespace SimpleRpc.Transports.Http.Client
{
    public class HttpClientTransport : BaseClientTransport
    {
        private readonly HttpClient _httpClient;
        private readonly IMessageSerializer _serializer;

        public HttpClientTransport(HttpClientTransportOptions options)
        {
            var url = new Uri(options.Url);

            _serializer = SerializationHelper.GetSerializer(options.Serializer);

            _httpClient = new HttpClient
            {
                BaseAddress = url
            };

            if (options.DefaultRequestHeaders != null)
            {
                foreach (var header in options.DefaultRequestHeaders)
                {
                    _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            _httpClient.DefaultRequestHeaders.Add(Constants.Other.ApplicationName, options.ApplicationName);
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;
            _httpClient.DefaultRequestHeaders.Host = url.Host;
        }

        public override object HandleSync(RpcRequest rpcRequest)
        {
            return SendRequest(rpcRequest).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override async Task HandleAsync(RpcRequest rpcRequest)
        {
            await SendRequest(rpcRequest).ConfigureAwait(false);
        }

        public override async Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest)
        {
            var result = await SendRequest(rpcRequest).ConfigureAwait(false);

            return (T)result;
        }

        private async Task<object> SendRequest(RpcRequest rpcRequest)
        {
            using (var memoryStream = new MemoryStream())
            {
                _serializer.Serialize(rpcRequest, memoryStream, typeof(RpcRequest));
                memoryStream.Position = 0;

                var streamContent = new StreamContent(memoryStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(_serializer.ContentType);

                using (var httpResponseMessage = await _httpClient.PostAsync(string.Empty, streamContent, CancellationToken.None).ConfigureAwait(false))
                {
                    var resultSerializer = SerializationHelper.GetSerializer(httpResponseMessage.Content.Headers.ContentType.MediaType);
                    var stream = await httpResponseMessage.Content.ReadAsStreamAsync();

                    switch (httpResponseMessage.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return resultSerializer.Deserialize(stream, typeof(object));
                        case HttpStatusCode.NoContent:
                            return null;
                        case HttpStatusCode.InternalServerError:
                            throw (Exception)resultSerializer.Deserialize(stream, typeof(Exception));
                        default:
                            throw new ArgumentException($"Not support HttpStatusCode: {httpResponseMessage.StatusCode}");
                    }
                }
            }
        }
    }
}
