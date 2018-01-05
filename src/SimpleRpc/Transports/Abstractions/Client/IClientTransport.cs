using System;

namespace SimpleRpc.Transports.Abstractions.Client
{
    public interface IClientTransport
    {
        object BuildProxy(Type t);
    }
}
