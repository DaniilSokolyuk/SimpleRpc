using System;
using MessagePack;

namespace SimpleRpc
{
    [MessagePackObject]
    public class RpcError
    {
        /// <summary>
        /// Gets or sets a numeric error code.
        /// </summary>
        /// <value>The code.</value>
        [Key(0)]
        public RpcErrorCode Code { get; set; }

        /// <summary>
        /// Gets or sets any server/method specific data provided about the error.
        /// </summary>
        /// <value>The data.</value>
        [Key(1)]
        public Exception Exception { get; set; }
    }
}
