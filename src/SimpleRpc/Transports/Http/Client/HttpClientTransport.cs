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
    internal class HttpClientTransport : BaseClientTransport
    {
        private readonly string _clientName;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMessageSerializer _serializer;
        private readonly ISerializationHelper serializationHelper;

        public HttpClientTransport(string clientName, IMessageSerializer serializer, ISerializationHelper serializationHelper, IHttpClientFactory httpClientFactory)
        {
            _clientName = clientName;
            _httpClientFactory = httpClientFactory;
            _serializer = serializer;
            this.serializationHelper = serializationHelper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override object HandleSync(RpcRequest rpcRequest) => WrapSendRequest<object>(rpcRequest).ConfigureAwait(false).GetAwaiter().GetResult();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task HandleAsync(RpcRequest rpcRequest) => WrapSendRequest<object>(rpcRequest);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Task<T> HandleAsyncWithResult<T>(RpcRequest rpcRequest) => WrapSendRequest<T>(rpcRequest);

        public Task<T> WrapSendRequest<T>(RpcRequest rpcRequest)
        {
            if (SynchronizationContext.Current != null)
            {
                return Task.Run(() => SendRequest<T>(rpcRequest));
            }

            return SendRequest<T>(rpcRequest);
        }

        private async Task<T> SendRequest<T>(RpcRequest rpcRequest)
        {
            var httpClient = _httpClientFactory.CreateClient(_clientName);

            using (var streamContent = new SerializbleContent(_serializer, rpcRequest))
            using (var httpResponseMessage = await httpClient.SendAsync(
                new HttpRequestMessage
                {
                    Content = streamContent,
                    Method = HttpMethod.Post,
                },
                HttpCompletionOption.ResponseHeadersRead,
                CancellationToken.None).ConfigureAwait(false)
            )
            {
                httpResponseMessage.EnsureSuccessStatusCode();

                var resultSerializer = serializationHelper.GetByContentType(httpResponseMessage.Content.Headers.ContentType.MediaType);
                var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);

                var result = (RpcResponse)await resultSerializer.DeserializeAsync(stream, typeof(RpcResponse)).ConfigureAwait(false);

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
            return _serializer.SerializeAsync(stream, _request, typeof(RpcRequest));
        }

        protected override bool TryComputeLength(out long length)
        {
            //we don't know. can't be computed up-front
            length = -1;
            return false;
        }
    }

}
