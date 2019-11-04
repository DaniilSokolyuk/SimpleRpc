using System;
using System.Collections.Generic;

namespace SimpleRpc.Transports.Http.Client
{
    public class HttpClientTransportOptions
    {
        public string Url { get; set; }

        public string ApplicationName { get; set; }

        public IDictionary<string, string> DefaultRequestHeaders { get; set; }

        public string Serializer { get; set; }
    }
}
