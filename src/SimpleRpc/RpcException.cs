using System;

namespace SimpleRpc
{
    public class RpcException : Exception
    {
        public RpcException(RpcError rpcError) : base($"SimpleRpc server exception: {rpcError.Code.ToString()}", rpcError.Exception)
        {
        }
    }
}
