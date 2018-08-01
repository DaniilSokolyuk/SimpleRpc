using System;

namespace SimpleRpc
{
    public class RpcException : Exception
    {
        public RpcException(RpcError rpcError)
        {
            RpcError = rpcError;
        }

        public RpcError RpcError { get; }
    }
}
