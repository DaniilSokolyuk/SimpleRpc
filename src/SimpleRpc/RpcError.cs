using System;

namespace SimpleRpc
{
    public class RpcError
    {
        /// <summary>
        /// Gets or sets a numeric error code.
        /// </summary>
        /// <value>The code.</value>
        public RpcErrorCode Code { get; set; }

        /// <summary>
        /// Gets or sets any server/method specific data provided about the error.
        /// </summary>
        /// <value>The data.</value>
        public Exception Exception { get; set; }
    }
}
