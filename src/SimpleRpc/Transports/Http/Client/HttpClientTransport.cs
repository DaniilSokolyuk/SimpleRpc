using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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

            _serializer = SerializationHelper.GetByName(options.Serializer);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object HandleSync(RpcRequest rpcRequest) => SendRequest<object>(rpcRequest).ConfigureAwait(false).GetAwaiter().GetResult();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task HandleAsync(RpcRequest rpcRequest) => SendRequest<object>(rpcRequest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest) => SendRequest<T>(rpcRequest);

        private async Task<T> SendRequest<T>(RpcRequest rpcRequest)
        {
            var streamContent = new SerializbleContent(_serializer, rpcRequest);

            using (var httpResponseMessage = await _httpClient.PostAsync(string.Empty, streamContent, CancellationToken.None).ConfigureAwait(false))
            {
                httpResponseMessage.EnsureSuccessStatusCode();

                var resultSerializer = SerializationHelper.GetByContentType(httpResponseMessage.Content.Headers.ContentType.MediaType);
                var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                var result = (RpcResponse)resultSerializer.Deserialize(stream, typeof(RpcResponse));

                if (result.Error != null)
                {
                    throw new RpcException(result.Error);
                }

                return (T)result.Result;
            }
        }
    }

    internal class SerializbleContent : HttpContent
    {
        private readonly IMessageSerializer _serializer;
        private readonly RpcRequest _request;

        public SerializbleContent(IMessageSerializer serializer, RpcRequest request)
        {
            _serializer = serializer;
            _request = request;

            Headers.ContentType = new MediaTypeHeaderValue(_serializer.ContentType);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            _serializer.Serialize(_request, stream, typeof(RpcRequest));
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            //we don't know. can't be computed up-front
            length = -1;
            return false;
        }
    }

}
